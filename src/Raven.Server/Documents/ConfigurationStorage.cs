﻿using System;
using System.Linq;
using Raven.Server.Documents.Operations;
using Raven.Server.NotificationCenter;
using Raven.Server.ServerWide.Context;
using Raven.Server.Storage.Layout;
using Raven.Server.Storage.Schema;
using Sparrow;
using Voron;

namespace Raven.Server.Documents
{
    public class ConfigurationStorage : IDisposable
    {
        public TransactionContextPool ContextPool { get; }

        public NotificationsStorage NotificationsStorage { get; }

        public OperationsStorage OperationsStorage { get; }

        public StorageEnvironment Environment { get; }

        public ConfigurationStorage(DocumentDatabase db)
        {
            var path = db.Configuration.Core.DataDirectory.Combine("Configuration");

            var options = db.Configuration.Core.RunInMemory
                ? StorageEnvironmentOptions.CreateMemoryOnly(path.FullPath, db.Configuration.Storage.TempPath?.FullPath, db.IoChanges, db.CatastrophicFailureNotification)
                : StorageEnvironmentOptions.ForPath(path.FullPath, db.Configuration.Storage.TempPath?.FullPath, null, db.IoChanges, db.CatastrophicFailureNotification);

            options.OnNonDurableFileSystemError += db.HandleNonDurableFileSystemError;
            options.OnRecoveryError += db.HandleOnRecoveryError;
            options.CompressTxAboveSizeInBytes = db.Configuration.Storage.CompressTxAboveSize.GetValue(SizeUnit.Bytes);
            options.SchemaVersion = SchemaUpgrader.CurrentVersion.ConfigurationVersion;
            options.SchemaUpgrader = SchemaUpgrader.Upgrader(SchemaUpgrader.StorageType.Configuration, this, null);
            options.ForceUsing32BitsPager = db.Configuration.Storage.ForceUsing32BitsPager;
            options.TimeToSyncAfterFlashInSec = (int)db.Configuration.Storage.TimeToSyncAfterFlash.AsTimeSpan.TotalSeconds;
            options.NumOfConcurrentSyncsPerPhysDrive = db.Configuration.Storage.NumberOfConcurrentSyncsPerPhysicalDrive;
            options.MasterKey = db.MasterKey?.ToArray();
            options.DoNotConsiderMemoryLockFailureAsCatastrophicError = db.Configuration.Security.DoNotConsiderMemoryLockFailureAsCatastrophicError;

            Environment = LayoutUpdater.OpenEnvironment(options);

            NotificationsStorage = new NotificationsStorage(db.Name);

            OperationsStorage = new OperationsStorage();

            ContextPool = new TransactionContextPool(Environment);
        }

        public void Initialize()
        {
            NotificationsStorage.Initialize(Environment, ContextPool);
            OperationsStorage.Initialize(Environment, ContextPool);
        }

        public void Dispose()
        {
            ContextPool?.Dispose();
            Environment.Dispose();
        }
    }
}
