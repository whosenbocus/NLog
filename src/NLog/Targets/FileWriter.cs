using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NLog.Common;
using NLog.Layouts;
using NLog.Internal;
using NLog.Targets.FileAppenders;

namespace NLog.Targets
{
    internal class FileWriter
    {
        private readonly FileTarget _fileTarget;
        private readonly FileNameBuilder _fileNameBuilder;
        private readonly Dictionary<string, FileTarget.OpenFileAppender> _openFileCache;
        private readonly Action<string, FileTarget.OpenFileAppender> _closeFile;
        private readonly Action<string, FileTarget.OpenFileAppender, bool> _closeFileWithFooter;
        private readonly Func<string, LogEventInfo, DateTime?, int, FileTarget.OpenFileAppender> _openFile;
        private readonly Func<string, FileTarget.OpenFileAppender, LogEventInfo, bool, FileTarget.OpenFileAppender> _rollArchiveFile;
        private readonly Func<FileTarget, IFileAppender, LogEventInfo, bool> _mustArchiveFile;
        private readonly Func<FileTarget.OpenFileAppender, ArraySegment<byte>, bool> _writeBytesAndFlush;

        public FileWriter(
            FileTarget fileTarget,
            FileNameBuilder fileNameBuilder,
            Dictionary<string, FileTarget.OpenFileAppender> openFileCache,
            Action<string, FileTarget.OpenFileAppender> closeFile,
            Action<string, FileTarget.OpenFileAppender, bool> closeFileWithFooter,
            Func<string, LogEventInfo, DateTime?, int, FileTarget.OpenFileAppender> openFile,
            Func<string, FileTarget.OpenFileAppender, LogEventInfo, bool, FileTarget.OpenFileAppender> rollArchiveFile,
            Func<FileTarget, IFileAppender, LogEventInfo, bool> mustArchiveFile,
            Func<FileTarget.OpenFileAppender, ArraySegment<byte>, bool> writeBytesAndFlush)
        {
            _fileTarget = fileTarget;
            _fileNameBuilder = fileNameBuilder;
            _openFileCache = openFileCache;
            _closeFile = closeFile;
            _closeFileWithFooter = closeFileWithFooter;
            _openFile = openFile;
            _rollArchiveFile = rollArchiveFile;
            _mustArchiveFile = mustArchiveFile;
            _writeBytesAndFlush = writeBytesAndFlush;
        }

        public Exception? WriteToFile(string filename, LogEventInfo firstLogEvent, MemoryStream ms)
        {
            try
            {
                ArraySegment<byte> bytes = new ArraySegment<byte>(ms.GetBuffer(), 0, (int)ms.Length);
                WriteBytesToFile(filename, firstLogEvent, bytes);
                return null;
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "{0}: Failed writing to FileName: '{1}'", _fileTarget, filename);
                if (_fileTarget.ExceptionMustBeRethrown(ex))
                    throw;
                return ex;
            }
        }

        public void WriteBytesToFile(string filename, LogEventInfo firstLogEvent, ArraySegment<byte> bytes)
        {
            bool hasWritten = true;
            if (!_openFileCache.TryGetValue(filename, out var openFile))
            {
                hasWritten = false;
                openFile = _openFile(filename, firstLogEvent, null, 0);
            }

            try
            {
                openFile = _rollArchiveFile(filename, openFile, firstLogEvent, hasWritten);
                _writeBytesAndFlush(openFile, bytes);
            }
            catch
            {
                _openFileCache.Remove(filename);
                openFile.FileAppender.Dispose();
                throw;
            }
            finally
            {
                _fileTarget._lastWriteTime = firstLogEvent.TimeStamp;
            }
        }
    }
}
