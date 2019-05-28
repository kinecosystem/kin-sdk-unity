package kin.unity;

import android.app.Activity;
import android.os.Bundle;
import android.content.Intent;
import android.util.Log;


public class BackupLauncher extends Activity {

    KinPlugin _plugin = KinPlugin.instance();

    String managerId;
    String clientId;
    String accountId;

    // Codes for backup and restore
    private static final int REQ_CODE_BACKUP = 9000;
    private static final int REQ_CODE_RESTORE = 9001;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        Intent intent = getIntent();
        String action = intent.getAction();
        managerId = intent.getStringExtra("managerId");
        clientId = intent.getStringExtra("clientId");
        if (action.equals(KinPlugin.BACKUP_ACTION))
        {
            accountId = intent.getStringExtra("accountId");
            _plugin.backupAccount(accountId, clientId, managerId);
        }
        else if (action.equals(KinPlugin.RESTORE_ACTION))
        {
            _plugin.restoreAccount(clientId, managerId);
        }
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode == REQ_CODE_BACKUP || requestCode == REQ_CODE_RESTORE) {
            _plugin._backupManagers.get(managerId).onActivityResult(requestCode, resultCode, data);
            finish();
        }
    }


}
