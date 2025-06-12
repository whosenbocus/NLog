using System;
using System.Collections.Generic;
using System.Threading;

namespace NLog.Targets
{
    internal class OpenFileCacheManager
    {
        private readonly Dictionary<string, FileTarget.OpenFileAppender> _openFileCache;
        private readonly int _openFileCacheSize;
        private readonly Action<string, FileTarget.OpenFileAppender, bool> _closeFileWithFooter;
        private readonly Action<string, FileTarget.OpenFileAppender> _closeFile;
        private readonly Timer? _openFileMonitorTimer;
        private readonly object _syncRoot;
        private readonly Func<int> _openFileMonitorTimerInterval;
        private readonly Func<int> _openFileCacheTimeout;
        private readonly Func<int> _openFileFlushTimeout;
        private readonly Func<bool> _autoFlush;
        private readonly Func<DateTime> _getCurrentTime;
        private readonly Func<DateTime> _getLastWriteTime;
        private readonly Action<FileTarget.OpenFileAppender> _flushFile;

        public OpenFileCacheManager(
            Dictionary<string, FileTarget.OpenFileAppender> openFileCache,
            int openFileCacheSize,
            Action<string, FileTarget.OpenFileAppender, bool> closeFileWithFooter,
            Action<string, FileTarget.OpenFileAppender> closeFile,
            Timer? openFileMonitorTimer,
            object syncRoot,
            Func<int> openFileMonitorTimerInterval,
            Func<int> openFileCacheTimeout,
            Func<int> openFileFlushTimeout,
            Func<bool> autoFlush,
            Func<DateTime> getCurrentTime,
            Func<DateTime> getLastWriteTime,
            Action<FileTarget.OpenFileAppender> flushFile)
        {
            _openFileCache = openFileCache;
            _openFileCacheSize = openFileCacheSize;
            _closeFileWithFooter = closeFileWithFooter;
            _closeFile = closeFile;
            _openFileMonitorTimer = openFileMonitorTimer;
            _syncRoot = syncRoot;
            _openFileMonitorTimerInterval = openFileMonitorTimerInterval;
            _openFileCacheTimeout = openFileCacheTimeout;
            _openFileFlushTimeout = openFileFlushTimeout;
            _autoFlush = autoFlush;
            _getCurrentTime = getCurrentTime;
            _getLastWriteTime = getLastWriteTime;
            _flushFile = flushFile;
        }

        public void PruneOpenFileCache()
        {
            RemoveDeletedFilesFromCache();
            RemoveOldestFilesIfCacheFull();
        }

        private void RemoveDeletedFilesFromCache()
        {
            while (_openFileCache.Count > 0)
            {
                KeyValuePair<string, FileTarget.OpenFileAppender> openFileDeleted = default;
                foreach (var openFile in _openFileCache)
                {
                    if (!openFile.Value.FileAppender.VerifyFileExists())
                    {
                        openFileDeleted = openFile;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(openFileDeleted.Key))
                    break;

                _closeFile(openFileDeleted.Key, openFileDeleted.Value);
            }
        }

        private void RemoveOldestFilesIfCacheFull()
        {
            while (_openFileCache.Count >= _openFileCacheSize)
            {
                DateTime oldestFileTime = DateTime.MaxValue;
                KeyValuePair<string, FileTarget.OpenFileAppender> oldestOpenFile = default;
                foreach (var oldOpenFile in _openFileCache)
                {
                    if (oldOpenFile.Value.FileAppender.OpenStreamTime < oldestFileTime)
                    {
                        oldestOpenFile = oldOpenFile;
                    }
                }
                if (!string.IsNullOrEmpty(oldestOpenFile.Key))
                    break;

                _closeFileWithFooter(oldestOpenFile.Key, oldestOpenFile.Value, false);
            }
        }

        public void PruneOpenFileCacheUsingTimeout()
        {
            DateTime closeTime = _getCurrentTime().AddSeconds(-_openFileCacheTimeout());
            bool oldFilesMustBeClosed = false;

            foreach (var openFile in _openFileCache)
            {
                if (openFile.Value.FileAppender.OpenStreamTime < closeTime)
                {
                    oldFilesMustBeClosed = true;
                    break;
                }
            }

            if (oldFilesMustBeClosed)
            {
                // Manual copy to list to avoid LINQ dependency
                var openFilesCopy = new List<KeyValuePair<string, FileTarget.OpenFileAppender>>(_openFileCache.Count);
                foreach (var openFile in _openFileCache)
                {
                    openFilesCopy.Add(openFile);
                }
                foreach (var openFile in openFilesCopy)
                {
                    if (openFile.Value.FileAppender.OpenStreamTime < closeTime)
                    {
                        _closeFile(openFile.Key, openFile.Value);
                    }
                }
            }
        }

        public void FlushOpenFilesIfNeeded()
        {
            DateTime flushTime = _getCurrentTime().AddSeconds(-(_openFileFlushTimeout() + 1) * 1.5);
            if (_getLastWriteTime() > flushTime)
            {
                foreach (var openFile in _openFileCache)
                {
                    _flushFile(openFile.Value);
                }
            }
        }
    }
}
