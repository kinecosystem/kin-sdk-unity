package kin.unity;

import android.app.Activity;
import android.os.Bundle;
import android.content.Intent;
import android.util.Log;

import java.util.Map;

import kin.backupandrestore.BackupAndRestoreManager;
import kin.backupandrestore.BackupCallback;
import kin.backupandrestore.RestoreCallback;
import kin.backupandrestore.exception.BackupAndRestoreException;
import kin.sdk.KinAccount;
import kin.sdk.KinClient;


public class BackupActivity extends Activity {

    KinPlugin _plugin = KinPlugin.instance();
    String LOGTAG = KinPlugin.TAG + ".Backup";
    BackupAndRestoreManager backupManager;

    String managerId;
    String clientId;
    String accountId;

    private static final int REQ_CODE_BACKUP = 9000;
    private static final int REQ_CODE_RESTORE = 9001;


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        Intent intent = getIntent();
        String action = intent.getAction();
        clientId = intent.getStringExtra("clientId");

        if (action.equals(KinPlugin.BACKUP_ACTION))
        {
            accountId = intent.getStringExtra("accountId");
            backupAccount(accountId, clientId);
        }
        else if (action.equals(KinPlugin.RESTORE_ACTION))
        {
            restoreAccount(clientId);
        }
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode == REQ_CODE_BACKUP || requestCode == REQ_CODE_RESTORE) {
            backupManager.onActivityResult(requestCode, resultCode, data);
            finish();
        }
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        if (backupManager != null)
        {
            releaseBackupManager();
        }
    }

    public BackupAndRestoreManager createBackupAndRestoreManager(String clientId, String accountId)
    {
        BackupAndRestoreManager backupManager = new BackupAndRestoreManager
                (this, REQ_CODE_BACKUP, REQ_CODE_RESTORE);

        backupManager.registerBackupCallback(new BackupCallback() {
            @Override
            public void onSuccess() {
                Log.d(LOGTAG, "Got success from backup manager");
                _plugin.unitySendMessage("BackupSucceeded", _plugin.callbackToJson(null, accountId));
            }

            @Override
            public void onCancel() {
                Log.d(LOGTAG, "Got cancel from backup manager");
                _plugin.unitySendMessage("BackupCanceled", _plugin.callbackToJson(null, accountId));
            }

            @Override
            public void onFailure(BackupAndRestoreException e) {
                Log.e(LOGTAG, "Got failure from backup manager");
                _plugin.unitySendMessage("BackupFailed", _plugin.exceptionToJson(e, accountId));
            }
        });

        backupManager.registerRestoreCallback(new RestoreCallback() {
            @Override
            public void onSuccess(KinClient kinClient, KinAccount kinAccount) {
                Log.d(LOGTAG, "Got success from backup manager");
                String accountId = _plugin.getAccountIdByAccount(kinAccount);
                // If we already have this account, there is no need to add it again to the list.
                // The account will still be added internally to the kin client until this bug will be fixed in the android b&r module
                if (accountId == null)
                {
                    accountId = _plugin.generateUniqueId();
                    _plugin._accounts.put(accountId, kinAccount);
                }
                else
                {
                    Log.d(LOGTAG, "Restored existing account");
                }
                _plugin.unitySendMessage("RestoreSucceeded", _plugin.callbackToJson(accountId, clientId));
            }

            @Override
            public void onCancel() {
                Log.d(LOGTAG, "Got cancel from backup manager");
                _plugin.unitySendMessage("RestoreCanceled", _plugin.callbackToJson(null, clientId));
            }

            @Override
            public void onFailure(BackupAndRestoreException e) {
                Log.e(LOGTAG, "Got failure from backup manager");
                _plugin.unitySendMessage("RestoreFailed", _plugin.exceptionToJson(e, clientId));
            }
        });

        Log.d(LOGTAG, "Created Kin backup manager");
        return backupManager;

    }

    protected void backupAccount(String accountId, String clientId)
    {
        try
        {
            backupManager = createBackupAndRestoreManager(clientId, accountId);
            KinClient client = _plugin._clients.get(clientId);
            KinAccount account = _plugin._accounts.get(accountId);

            backupManager.backup(client, account);
        }
        catch (Exception e)
        {
            e.printStackTrace();
            Log.e( LOGTAG, "backupAccount failed", e );
            _plugin.unitySendMessage("BackupFailed", _plugin.exceptionToJson(e, managerId));
            finish();
        }
    }

    protected void restoreAccount(String clientId)
    {
        try
        {
            backupManager = createBackupAndRestoreManager(clientId, null);
            KinClient client = _plugin._clients.get(clientId);

            backupManager.restore(client);
        }
        catch (Exception e)
        {
            e.printStackTrace();
            Log.e( LOGTAG, "restoreAccount failed", e );
            _plugin.unitySendMessage("onRestore", _plugin.exceptionToJson(e, managerId));
            finish();
        }
    }

    public void releaseBackupManager()
    {
        try
        {
            backupManager.release();
        }
        catch (Exception e)
        {
            e.printStackTrace();
            Log.e( LOGTAG, "releaseBackupManager failed", e );
        }
    }
}
