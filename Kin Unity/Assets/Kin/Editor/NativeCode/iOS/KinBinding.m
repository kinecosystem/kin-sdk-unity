#import "KinPlugin-Generated-Swift.h"

#define MakeStringCopy( _x_ ) ( _x_ != NULL && [_x_ isKindOfClass:[NSString class]] ) ? strdup( [_x_ UTF8String] ) : NULL
#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]
#define GetStringParamOrNil( _x_ ) ( _x_ != NULL && strlen( _x_ ) ) ? [NSString stringWithUTF8String:_x_] : nil


void _kinCreateClient( const char* clientId, int environment, const char* apiKey, const char* storeKey )
{
	[KinPlugin.instance createClientWithClientId:GetStringParam( clientId )
									 environment:environment
										  apiKey:GetStringParam( apiKey )
										storeKey:GetStringParamOrNil( storeKey )];
}


void _kinFreeCachedClient( const char* clientId )
{
	[KinPlugin.instance freeCachedClientWithClientId:GetStringParam( clientId )];
}


const char* _kinImportAccount( const char* clientId, const char* accountId, const char* exportedJson, const char* passphrase )
{
	NSString* res = [KinPlugin.instance importAccountWithClientId:GetStringParam( clientId )
														accountId:GetStringParam( accountId )
													 exportedJson:GetStringParam( exportedJson )
													   passphrase:GetStringParam( passphrase )];
	return MakeStringCopy( res );
}


int _kinGetAccountCount( const char* clientId )
{
	return (int)[KinPlugin.instance getAccountCountWithClientId:GetStringParam( clientId )];
}


const char* _kinAddAccount( const char* clientId, const char* accountId )
{
	NSString* res = [KinPlugin.instance addAccountWithClientId:GetStringParam( clientId )
													 accountId:GetStringParam( accountId )];
	return MakeStringCopy( res );
}


bool _kinGetAccount( const char* clientId, const char* accountId, int index )
{
	return [KinPlugin.instance getAccountWithClientId:GetStringParam( clientId )
											accountId:GetStringParam( accountId )
												index:index];
}


const char* _kinDeleteAccount( const char* clientId, int index )
{
	NSString* res = [KinPlugin.instance deleteAccountWithClientId:GetStringParam( clientId ) index:index];
	return MakeStringCopy( res );
}


void _kinClearAllAccounts( const char* clientId )
{
	[KinPlugin.instance clearAllAccountsWithClientId:GetStringParam( clientId )];
}


void _kinGetMinimumFee( const char* client )
{
	[KinPlugin.instance getMinimumFeeWithClientId:GetStringParam( client )];
}


void _kinFreeCachedAccount( const char* accountId )
{
	[KinPlugin.instance freeCachedClientWithClientId:GetStringParam( accountId )];
}


const char* _kinGetPublicAddress( const char* accountId )
{
	NSString* res = [KinPlugin.instance getPublicAddressWithAccountId:GetStringParam( accountId )];
	return MakeStringCopy( res );
}


const char* _kinExport( const char* accountId, const char* passphrase )
{
	NSString* res = [KinPlugin.instance exportWithAccountId:GetStringParam( accountId ) passphrase:GetStringParam( passphrase )];
	return MakeStringCopy( res );
}


void _kinGetStatus( const char* accountId )
{
	[KinPlugin.instance getStatusWithAccountId:GetStringParam( accountId )];
}


void _kinGetBalance( const char* accountId )
{
	[KinPlugin.instance getBalanceWithAccountId:GetStringParam( accountId )];
}


void _kinBuildTransaction( const char* accountId, const char* toAddress, const char* kinAmount, int fee, const char* memo )
{
	[KinPlugin.instance buildTransactionWithAccountId:GetStringParam( accountId )
											toAddress:GetStringParam( toAddress )
											kinAmount:GetStringParam( kinAmount )
												  fee:fee
												 memo:GetStringParamOrNil( memo )];
}


void _kinSendWhitelistTransaction( const char* accountId, const char* transactionId, const char* whitelist )
{
	[KinPlugin.instance sendWhitelistTransactionWithAccountId:GetStringParam( accountId )
														   id:GetStringParam( transactionId )
													whitelist:GetStringParam( whitelist )];
}


void _kinSendTransaction( const char* accountId, const char* transactionId )
{
	[KinPlugin.instance sendTransactionWithAccountId:GetStringParam( accountId ) id:GetStringParam( transactionId )];
}


void _kinAddPaymentListener( const char* accountId )
{
	[KinPlugin.instance addPaymentListenerWithAccountId:GetStringParam( accountId )];
}


void _kinRemovePaymentListener( const char* accountId )
{
	[KinPlugin.instance removePaymentListenerWithAccountId:GetStringParam( accountId )];
}


void _kinAddBalanceListener( const char* accountId )
{
	[KinPlugin.instance addBalanceListenerWithAccountId:GetStringParam( accountId )];
}


void _kinRemoveBalanceListener( const char* accountId )
{
	[KinPlugin.instance removeBalanceListenerWithAccountId:GetStringParam( accountId )];
}


void _kinAddAccountCreationListener( const char* accountId )
{
	[KinPlugin.instance addAccountCreationListenerWithAccountId:GetStringParam( accountId )];
}


void _kinRemoveAccountCreationListener( const char* accountId )
{
	[KinPlugin.instance removeAccountCreationListenerWithAccountId:GetStringParam( accountId )];
}

void _kinRestoreAccount(const char* clientId)
{
    [KinPlugin.instance restoreAccountWithClientId:GetStringParam( clientId )];
}

void _kinBackupAccount(const char* accountId)
{
    [KinPlugin.instance backupAccountWithAccountId:GetStringParam( accountId )];
}

