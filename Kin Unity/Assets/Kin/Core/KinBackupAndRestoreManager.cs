using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kin
{
    public enum BackupRestoreResult
    {
        Success = 0,
        Failed = 1,
        Cancel = 2
    }

    /// <summary>
    /// represents a native KinBackupAndRestoreManager. This class will call the correct native code based on the currently running platform.
    /// </summary>
    public class KinBackupAndRestoreManager
    {
        readonly internal string _backupManagerId;

        /// <summary>
        /// creates a new KinBackupAndRestoreManager object
        /// </summary>
        public KinBackupAndRestoreManager()
        {
            _backupManagerId = Utils.RandomString();
            string error = NativeBridge.Get().CreateBackupAndRestoreManager(_backupManagerId);

            if (!string.IsNullOrEmpty(error))
                throw KinException.FromNativeErrorJson(error);
        }

        //~KinBackupAndRestoreManager()
            //{
            //string error = NativeBridge.Get().ReleaseBackupManager(_backupManagerId);
            //if (!string.IsNullOrEmpty(error))
                //Debug.LogError("ReleaseBackupManager failed, " + JsonUtility.FromJson<KinException>(error));
        //}

        /// <summary>
        /// restore and account that was backed up
        /// </summary>
        /// <param name="onComplete"></param>
        /// <returns></returns>
        public void Restore(KinClient kinClient, Action<KinException, BackupRestoreResult, KinAccount> onComplete)
        {
            if (KinManager.onRestore.ContainsKey(_backupManagerId))
                throw new KinException("KinBackupAndRestoreManager request already in flight for this method. Wait for it to complete before requesting it again.");
            KinManager.onRestore[_backupManagerId] = onComplete;

            NativeBridge.Get().RestoreAccount(kinClient._clientId, _backupManagerId);
        }

        /// <summary>
        /// backup an account which can be restored later
        /// </summary>
        /// <param name="kinClient"></param>
        /// <param name="onComplete"></param>
        /// <returns></returns>
        public void Backup(KinAccount kinAccount,KinClient kinClient, Action<KinException, BackupRestoreResult> onComplete)
        {
            if (KinManager.onBackup.ContainsKey(_backupManagerId))
                throw new KinException("KinBackupAndRestoreManager request already in flight for this method. Wait for it to complete before requesting it again.");
            KinManager.onBackup[_backupManagerId] = onComplete;

            NativeBridge.Get().BackupAccount(kinAccount._accountId, kinClient._clientId, _backupManagerId);
        }

    }
}