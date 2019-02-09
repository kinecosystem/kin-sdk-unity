//
//  KinPlugin.swift
//  Unity-iPhone
//
//  Created by desaro on 11/2/18.
//

import Foundation
import KinSDK


struct Provider: ServiceProvider {
	public let url: URL
	public let network: Network
	
	static public let Test = Provider( url: URL( string: "http://horizon-testnet.kininfrastructure.com" )!, network: .testNet )
	static public let Main = Provider( url: URL( string: "https://horizon-ecosystem.kininfrastructure.com" )!, network: .mainNet )
	
	
	init( url: URL, network: Network ) {
		self.url = url
		self.network = network
	}
}


@objc class KinPlugin : NSObject {
	@objc static let instance = KinPlugin()
	
	var clients: Dictionary<String,KinClient> = Dictionary<String,KinClient>()
	var accounts: Dictionary<String,KinAccount> = Dictionary<String,KinAccount>()
	var transactions: Dictionary<String,TransactionEnvelope> = Dictionary<String,TransactionEnvelope>()
	
	var paymentListeners = Dictionary<String,PaymentWatch>()
	var balanceListeners = Dictionary<String,BalanceWatch>()
	var accountListeners = Dictionary<String,Promise<Void>>();
	
	let formatter = NumberFormatter()
	
	
	override init()
	{
		formatter.generatesDecimalNumbers = true
	}
	
	
	private func unitySendMessage( method: String, param: String ) {
		UnitySendMessage( "KinManager", method, param )
	}
	
	
	private func errorToJson( error: Error, accountId: String? ) -> String {
		var dict = ["Message": "", "NativeType": "", "ErrorCode": NSNumber.init( value: 1 )] as [String : Any]
		
		if( accountId != nil ) {
			dict["AccountId"] = accountId
		}
		
		do {
			let jsonData = try JSONSerialization.data( withJSONObject: dict, options: [] )
			let jsonString = String( data: jsonData, encoding: .utf8 )
			
			return jsonString!
		}
		catch {
			print( "Error converting error to JSON: \(error)" )
			return "{}"
		}
	}
	
	
	private func callbackToJson( accountId: String, value: String ) -> String {
		let dict = ["AccountId": accountId, "Value": value]
		
		do {
			let jsonData = try JSONSerialization.data( withJSONObject: dict, options: [] )
			let jsonString = String( data: jsonData, encoding: .utf8 )
			
			return jsonString!
		}
		catch {
			return "{}"
		}
	}
	
	
	private func paymentInfoToJson( paymentInfo: PaymentInfo, accountId: String ) -> String {
		let dateFormatter = DateFormatter()
		dateFormatter.dateFormat = "yyyy-MM-dd hh:mm:ss"
		
		var dict = ["AccountId": accountId] as [String : Any]
		dict["_Amount"] = formatter.string( from: paymentInfo.amount as NSDecimalNumber )
		dict["CreatedAt"] = dateFormatter.string( from: paymentInfo.createdAt )
		dict["DestinationPublicKey"] = paymentInfo.destination
		dict["SourcePublicKey"] = paymentInfo.source
		dict["Hash"] = paymentInfo.hash
		dict["Memo"] = paymentInfo.memoText
		
		do {
			let jsonData = try JSONSerialization.data( withJSONObject: dict, options: [] )
			let jsonString = String( data: jsonData, encoding: .utf8 )
			
			return jsonString!
		}
		catch {
			return "{}"
		}
	}
	
	
	private func transactionToJson( transaction: TransactionEnvelope, accountId: String ) -> String {
		//		json.put( "Id", transaction.getId().id() );
		//		json.put( "WhitelistableTransactionPayLoad", transaction.getWhitelistableTransaction().getTransactionPayload() );
		//		json.put( "WhitelistableTransactionNetworkPassphrase", transaction.getWhitelistableTransaction().getNetworkPassphrase() );
		
		// TODO
//		let wtf = try? JSONEncoder().encode(transaction)
//		print(String(data: wtf!, encoding: .utf8)!)
		
		var dict = ["AccountId": accountId] as [String : Any]
		//dict["Id"] = transaction.tx.seqNum.tostring()
		dict["WhitelistableTransactionPayLoad"] = ""
		dict["WhitelistableTransactionNetworkPassphrase"] = ""
		
		do {
			let jsonData = try JSONSerialization.data( withJSONObject: dict, options: [] )
			let jsonString = String( data: jsonData, encoding: .utf8 )
			
			return jsonString!
		}
		catch {
			return "{}"
		}
	}
	
	
	// MARK: - KinClient
	
	@objc public func createClient( clientId: String, environment: Int, apiKey: String, storeKey: String = "" )
	{
		let provider = environment == 0 ? Provider.Test : Provider.Main
		
		do {
			let client = KinClient( provider: provider, appId: try AppId( apiKey ) )
			self.clients[clientId] = client;
		}
		catch {
			print( "could not create KinClient: \(error)" )
		}
	}
	
	
	@objc public func freeCachedClient( clientId: String ) {
		if let _ = self.clients[clientId] {
			self.clients.removeValue( forKey: clientId )
		}
	}
	
	
	@objc public func importAccount( clientId: String, accountId: String, exportedJson: String, passphrase: String ) -> String? {
		do {
			if let client = self.clients[clientId] {
				let kinAccount = try client.importAccount(exportedJson, passphrase: passphrase)
				self.accounts[accountId] = kinAccount
				return "";
			}
		}
		catch {
			print( "KinAccount couldn't be imported: \(error)" )
			return errorToJson( error: error, accountId: accountId )
		}
		return nil;
	}
	
	
	@objc public func getAccountCount( clientId: String ) -> Int {
		return self.clients[clientId]?.accounts.count ?? -1
	}
	
	
	@objc public func addAccount( clientId: String, accountId: String ) -> String? {
		if let client = self.clients[clientId] {
			do {
				let account = try client.addAccount()
				self.accounts[accountId] = account
			}
			catch {
				print( "KinAccount couldn't be added: \(error)" )
				return errorToJson( error: error, accountId: accountId )
			}
		}
		return nil
	}
	
	
	@objc public func getAccount( clientId: String, accountId: String, index: Int ) -> Bool {
		if let client = self.clients[clientId] {
			let account = client.accounts[index]
			self.accounts[accountId] = account
		}
		return true
	}
	
	
	@objc public func deleteAccount( clientId: String, index: Int ) -> String? {
		if let client = self.clients[clientId] {
			let account = client.accounts[index]
			
			do {
				try client.deleteAccount( at: index )
				
				// now, remove the account from our cache
				var foundKey: String?
				for key in self.accounts.keys {
					let temp = self.accounts[key]
					if temp?.publicAddress == account?.publicAddress {
						foundKey = key
						break
					}
				}
				
				if foundKey != nil {
					self.accounts.removeValue( forKey: foundKey! )
				}
			}
			catch {
				print( "KinAccount couldn't be deleted: \(error)" )
				return errorToJson( error: error, accountId: nil )
			}
		}
		return nil
	}
	
	
	@objc public func clearAllAccounts( clientId: String ) {
		if let client = self.clients[clientId] {
			client.deleteKeystore()
		}
	}
	
	
	// MARK: - KinAccount
	
	@objc public func freeCachedAccount( accountId: String ) {
		if self.accounts.keys.contains( accountId ) {
			self.accounts.removeValue( forKey: accountId )
		}
	}
	
	
	@objc public func getPublicAddress( accountId: String ) -> String {
		return (self.accounts[accountId]?.publicAddress)!
	}
	
	
	@objc public func export( accountId: String, passphrase: String ) -> String? {
		if let account = self.accounts[accountId] {
			do {
				return try account.export( passphrase: passphrase )
			}
			catch {
				print( "KinAccount couldn't be exported: \(error)" )
				return errorToJson( error: error, accountId: nil )
			}
		}
		return nil
	}
	
	@objc public func getStatus( accountId: String ) {
		if let account = self.accounts[accountId] {
			//account.status(completion:
			account.status { (accountStatus, error) in
				if let error = error {
					self.unitySendMessage( method: "GetStatusFailed", param: self.errorToJson( error: error, accountId: accountId ) )
				} else {
					var status:String;
					switch(accountStatus!)
					{
						case .created:
							status = "Created";
						case .notCreated:
							status = "NotCrated";
					}
					self.unitySendMessage( method: "GetStatusSucceeded", param: self.callbackToJson(accountId: accountId, value: status) )
				}
			}
		}
	}
	
	
	@objc public func getBalance( accountId: String ) {
		if let account = self.accounts[accountId] {
			account.balance { (kin, error) in
				if let error = error {
					self.unitySendMessage( method: "GetBalanceFailed", param: self.errorToJson( error: error, accountId: accountId ) )
				} else {
					self.unitySendMessage( method: "GetBalanceSucceeded", param: self.callbackToJson(accountId: accountId, value: (kin?.description)!) )
				}
			}
		}
	}
	
	
	@objc public func buildTransaction( accountId: String, toAddress: String, kinAmount: String, fee: UInt32, memo: String! = nil ) {
		if let account = self.accounts[accountId] {
			account.generateTransaction(to: toAddress, kin: Decimal(string: kinAmount)!, memo: memo, fee: fee) { (transaction, error) in
				if let error = error {
					self.unitySendMessage( method: "BuildTransactionFailed", param: self.errorToJson( error: error, accountId: accountId ) )
				} else {
					// TODO:
					self.transactions["TODO"] = transaction
					self.unitySendMessage( method: "BuildTransactionSucceeded", param: self.transactionToJson(transaction: transaction!, accountId: accountId) )
				}
			}
		}
	}
	
	
	@objc public func sendTransaction( accountId: String, id: String ) {
		if let account = self.accounts[accountId] {
			if let transaction = self.transactions[id] {
				account.sendTransaction(transaction) { (transactionId, error) in
					if let error = error {
						self.unitySendMessage( method: "SendTransactionFailed", param: self.errorToJson( error: error, accountId: accountId ) )
					} else {
						self.unitySendMessage( method: "SendTransactionSucceeded", param: self.callbackToJson(accountId: accountId, value: transactionId!) )
					}
				}
			}
		}
	}
	
	
	// MARK: - KinAccount Listeners
	
	@objc public func addPaymentListener( accountId: String ) {
		if let account = self.accounts[accountId] {
			let watch = try? account.watchPayments(cursor: nil)
			watch?.emitter.on(queue: .main, next: { [weak self] payment in
				let paymentInfoJson = self?.paymentInfoToJson( paymentInfo: payment, accountId: accountId)
				self?.unitySendMessage( method: "OnPayment", param: paymentInfoJson! )
			})
			
			self.paymentListeners[accountId] = watch
		}
	}
	
	
	@objc public func removePaymentListener( accountId: String ) {
		if self.paymentListeners.keys.contains( accountId ) {
			self.paymentListeners.removeValue( forKey: accountId )
		}
	}
	
	
	@objc public func addBalanceListener( accountId: String ) {
		if let account = self.accounts[accountId] {
			let watch = try? account.watchBalance(nil)
			watch?.emitter.on(queue: .main, next: { [weak self] balance in
				let callbackJson = self?.callbackToJson(accountId: accountId, value: balance.description)
				self?.unitySendMessage( method: "OnBalance", param: callbackJson! )
			})
			
			self.balanceListeners[accountId] = watch
		}
	}
	
	
	@objc public func removeBalanceListener( accountId: String ) {
		if self.balanceListeners.keys.contains( accountId ) {
			self.balanceListeners.removeValue( forKey: accountId )
		}
	}
	
	
	@objc public func addAccountCreationListener( accountId: String ) {
		if let account = self.accounts[accountId] {
			let watch = try? account.watchCreation()
			watch?.finally {
				self.unitySendMessage( method: "OnAccountCreated", param: accountId )
			}
			
			self.accountListeners[accountId] = watch
		}
	}
	
	
	@objc public func removeAccountCreationListener( accountId: String ) {
		if self.accountListeners.keys.contains( accountId ) {
			self.accountListeners.removeValue( forKey: accountId )
		}
	}
	
}
