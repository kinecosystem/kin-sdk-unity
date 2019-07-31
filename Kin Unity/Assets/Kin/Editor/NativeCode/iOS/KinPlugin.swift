//
//  KinPlugin.swift
//  Unity-iPhone
//
//  Created by desaro on 11/2/18.
//
import UIKit
import Foundation
import KinSDK
import KinBackupRestoreModule
import Sodium


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

enum BackupAction {
    case backup(String)
    case restore(String)
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
	var networkId: String = ""
    
    let vc: UIViewController = UnityGetGLViewController()
    
    let backupRestoreManager = KinBackupRestoreManager()
    var lastBackupRestoreRequest: BackupAction?

	override init() {
        super.init()
		formatter.generatesDecimalNumbers = true
        self.backupRestoreManager.delegate = self
	}

	// MARK: - helpers
	
	private func unitySendMessage( method: String, param: String ) {
		UnitySendMessage( "KinManager", method, param )
	}
	
	
	private func errorToJson( error: Error, accountId: String? ) -> String {
		var dict = ["Message": error.localizedDescription, "NativeType": String(describing: error), "ErrorCode": NSNumber.init( value: 1 )] as [String : Any]
		
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
	
	
	private func transactionToJson( transaction: TransactionEnvelope, accountId: String, transactionId: String ) -> String {
		do {
			// do some hacking of the Decodable protocol to get the iOS data to match the Android data
			let whitelistEnvelope = WhitelistEnvelope(transactionEnvelope: transaction, networkId: self.networkId)
			let encodedEnvelope = try? JSONEncoder().encode(whitelistEnvelope)
			let whitelistDict = try! JSONSerialization.jsonObject(with: encodedEnvelope!, options: []) as? [String:String]
			let whitelistPayload = whitelistDict!["tx_envelope"]
			
			var dict = ["AccountId": accountId] as [String : Any]
			dict["Id"] = transactionId
			dict["WhitelistableTransactionPayLoad"] = whitelistPayload
			dict["WhitelistableTransactionNetworkPassphrase"] = Network.testNet.id

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
			let appId = try AppId(apiKey)
			let client = KinClient( provider: provider, appId: appId )
			self.clients[clientId] = client
			self.networkId = client.network.id
		}
		catch {
			print( "could not create KinClient: \(error)" )
		}
	}
	
	
	@objc public func getMinimumFee( clientId: String ) {
		if let client = self.clients[clientId] {
			client.minFee().then { (fee) in
				self.unitySendMessage( method: "GetMinimumFeeSucceeded", param: self.callbackToJson( accountId: clientId, value: fee.description ) )
			}.error { (error) in
				print( "getMinimumFee failed: \(error)" )
				self.unitySendMessage( method: "GetMinimumFeeFailed", param: self.errorToJson( error: error, accountId: clientId ) )
			}
		}
	}
	
	
	@objc public func freeCachedClient( clientId: String ) {
		if let _ = self.clients[clientId] {
			print( "Freeing cached client" )
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
			if let account = client.accounts[index] {
				self.accounts[accountId] = account
				return true
			}
		}
		return false
	}
	
	
	@objc public func deleteAccount( clientId: String, index: Int ) -> String? {
		if let client = self.clients[clientId] {
			let account = client.accounts[index]
			
			do {
				// find the key from our dict *before* deleting the account to avoid accessing a deleted account
				var foundKey: String?
				for key in self.accounts.keys {
					let temp = self.accounts[key]
					if temp?.publicAddress == account?.publicAddress {
						foundKey = key
						break
					}
				}
				
				try client.deleteAccount( at: index )

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
			print( "Freeing cached account" )
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
			account.status { (accountStatus, error) in
				if let error = error {
					print( "getStatus failed: \(error)" )
					self.unitySendMessage( method: "GetStatusFailed", param: self.errorToJson( error: error, accountId: accountId ) )
				} else {
					// normalize to match Android
					var statusInt = accountStatus!.rawValue
					if statusInt == 1 {
						statusInt += 1
					}
					self.unitySendMessage( method: "GetStatusSucceeded", param: self.callbackToJson(accountId: accountId, value: String( statusInt ) ) )
				}
			}
		}
	}
	
	
	@objc public func getBalance( accountId: String ) {
		if let account = self.accounts[accountId] {
			account.balance { (kin, error) in
				if let error = error {
					print( "getBalance failed: \(error)" )
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
					print( "buildTransaction failed: \(error)" )
					self.unitySendMessage( method: "BuildTransactionFailed", param: self.errorToJson( error: error, accountId: accountId ) )
				} else {
					do {
						let transactionIdHash = try transaction?.tx.hash(networkId: self.networkId)
						let transactionId = transactionIdHash!.hexString
						self.transactions[transactionId] = transaction
						self.unitySendMessage( method: "BuildTransactionSucceeded", param: self.transactionToJson(transaction: transaction!, accountId: accountId, transactionId: transactionId ) )
					} catch {
						print( "buildTransaction failed: \(error)" )
						self.unitySendMessage( method: "BuildTransactionFailed", param: self.errorToJson( error: error, accountId: accountId ) )
					}
				}
			}
		}
	}
	
	
	@objc public func sendTransaction( accountId: String, id: String ) {
		if let account = self.accounts[accountId] {
			if let transaction = self.transactions[id] {
				self.transactions.removeValue(forKey: id)
				account.sendTransaction(transaction) { (transactionId, error) in
					if let error = error {
						print( "sendTransaction failed: \(error)" )
						self.unitySendMessage( method: "SendTransactionFailed", param: self.errorToJson( error: error, accountId: accountId ) )
					} else {
						self.unitySendMessage( method: "SendTransactionSucceeded", param: self.callbackToJson(accountId: accountId, value: transactionId!) )
					}
				}
			}
		}
	}
	
	
	@objc public func sendWhitelistTransaction( accountId: String, id: String, whitelist: String ) {
		if let account = self.accounts[accountId] {
			if self.transactions[id] != nil {
				var envelope: TransactionEnvelope
				do {
					envelope = try XDRDecoder.decode(TransactionEnvelope.self, data: Data(base64Encoded: whitelist)!)
				} catch {
					print( "TransactionEnvelope.decodeResponse failed: \(error)" )
					self.unitySendMessage( method: "SendTransactionFailed", param: self.errorToJson( error: error, accountId: accountId ) )
					return
				}
				
				self.transactions.removeValue(forKey: id)
				account.sendTransaction(envelope) { (transactionId, error) in
					if let error = error {
						print( "sendTransaction failed: \(error)" )
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
    
    @objc public func restoreAccount( clientId: String ) {
        if let client = self.clients[clientId] {
            lastBackupRestoreRequest = BackupAction.restore(clientId);
            backupRestoreManager.restore(client, presentedOnto: self.vc)
        }
    }
    
    @objc public func backupAccount(accountId: String) {
        if let account = accounts[accountId] {
            lastBackupRestoreRequest = BackupAction.backup(accountId);
            backupRestoreManager.backup(account, presentedOnto: vc)
        }
    }
    
    private func generateUniqueId() -> String {
        // Generate a unique id (11 lowercase/numbers), equvilent of the android/c# methods in the Unity relevant code
        return String(NSUUID().uuidString.prefix(11))
    }
}

extension KinPlugin: KinBackupRestoreManagerDelegate {
    func kinBackupRestoreManagerDidComplete(_ manager: KinBackupRestoreManager, kinAccount: KinAccount?) {
        guard let request = lastBackupRestoreRequest else {
            return
        }
        
        switch request {
        case .backup(let accountId):
            unitySendMessage(method: "BackupSucceeded", param: callbackToJson(accountId: accountId, value: ""))
        case .restore(let clientId):
            guard let kAccount = kinAccount else {
                return
            }
            
            let accountId: String

            if let matchedId = accounts
                .first(where: { $1.publicAddress == kAccount.publicAddress})?
                .key {
                accountId = matchedId
            } else {
                accountId = generateUniqueId()
                accounts[accountId] = kAccount
            }

            unitySendMessage(method: "RestoreSucceeded",
                             param: callbackToJson(accountId: clientId, value: accountId))
                
        }
    }
    
    func kinBackupRestoreManagerDidCancel(_ manager: KinBackupRestoreManager) {
        guard let request = lastBackupRestoreRequest else {
            return
        }
        
        switch request {
        case .backup(let accountId):
            unitySendMessage(method: "BackupCanceled", param: self.callbackToJson(accountId: accountId, value: ""))
        case .restore(let clientId):
            unitySendMessage(method: "RestoreCanceled", param: self.callbackToJson(accountId: clientId, value: ""))
        }
    }
    
    func kinBackupRestoreManager(_ manager: KinBackupRestoreManager, error: Error) {
        guard let request = lastBackupRestoreRequest else {
            return
        }
        
        switch request {
        case .backup(let accountId):
            unitySendMessage(method: "BackupFailed", param:
                errorToJson(error: error, accountId: accountId))
        case .restore(let clientId):
            unitySendMessage(method: "RestoreFailed", param:
                errorToJson(error: error, accountId: clientId))
        }
    }
}
