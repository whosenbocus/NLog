using System;
using System.IO;
using System.Collections.Generic;
using NLog.Layouts;
using NLog.Internal;
using NLog.Common;
using NLog.Internal.Fakeables;

namespace NLog.Targets
{
    internal class FileNameBuilder
    {
        private readonly FileTarget _fileTarget;
        private static readonly char[] DirectorySeparatorChars = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        private static readonly HashSet<char> InvalidFileNameChars = new HashSet<char>(Path.GetInvalidFileNameChars());

        public FileNameBuilder(FileTarget fileTarget)
        {
            _fileTarget = fileTarget;
        }

        public string BuildFullFilePath(string newFileName, int sequenceNumber, DateTime fileLastModified = default)
        {
            if (sequenceNumber > 0 || fileLastModified != default)
            {
                var fileName = Path.GetFileName(newFileName) ?? string.Empty;
                var fileExt = Path.GetExtension(fileName) ?? string.Empty;
                newFileName = newFileName.Substring(0, newFileName.Length - fileName.Length);
                if (!string.IsNullOrEmpty(fileExt))
                    fileName = fileName.Substring(0, fileName.Length - fileExt.Length);

                object fileLastModifiedObj = fileLastModified == default ? string.Empty : (object)fileLastModified;
                try
                {
                    newFileName = newFileName + fileName + string.Format(_fileTarget.ArchiveSuffixFormat, sequenceNumber, fileLastModifiedObj) + fileExt;
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "{0}: Failed to apply ArchiveSuffixFormat={1} using SequenceNumber={2} for file: '{3}'", _fileTarget, _fileTarget.ArchiveSuffixFormat, sequenceNumber, newFileName);
                    if (_fileTarget.ExceptionMustBeRethrown(ex))
                        throw;
                    newFileName = newFileName + fileName + string.Format("_{0:00}", sequenceNumber) + fileExt;
                }
            }

            var filepath = CleanFullFilePath(newFileName);
            return filepath;
        }

        public string CleanFullFilePath(string filename)
        {
            var lastDirSeparator = filename.LastIndexOfAny(DirectorySeparatorChars);

            char[]? fileNameChars = null;
            for (int i = lastDirSeparator + 1; i < filename.Length; i++)
            {
                if (InvalidFileNameChars.Contains(filename[i]))
                {
                    if (fileNameChars is null)
                    {
                        fileNameChars = filename.Substring(lastDirSeparator + 1).ToCharArray();
                    }
                    fileNameChars[i - (lastDirSeparator + 1)] = '_';
                }
            }

            if (fileNameChars != null)
            {
                var dirName = lastDirSeparator > 0 ? filename.Substring(0, lastDirSeparator + 1) : string.Empty;
                filename = Path.Combine(dirName, new string(fileNameChars));
            }

            var filepath = FileInfoHelper.IsRelativeFilePath(filename) ? Path.Combine(AppEnvironmentWrapper.FixFilePathWithLongUNC(LogManager.LogFactory.CurrentAppEnvironment.AppDomainBaseDirectory), filename) : filename;
            filepath = Path.GetFullPath(filepath);
            return filepath;
        }
    }
}
