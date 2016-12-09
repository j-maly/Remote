using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileLock;
using JetBrains.Annotations;
using Remote.Utils;

namespace Remote
{
    public class Logging
    {
        private readonly Config config;
        private readonly SimpleFileLock fileLock;
        private readonly FileSystemWatcher logWatcher;

        public Logging(Config config)
        {
            this.config = config;
            if (!string.IsNullOrEmpty(config?.SharedLogFile))
            {
                fileLock = SimpleFileLock.Create(config.SharedLogFile + ".lock", TimeSpan.FromMinutes(1));
                logWatcher = new FileSystemWatcher(Path.GetDirectoryName(config.SharedLogFile), Path.GetFileName(config.SharedLogFile));
                logWatcher.Changed += WatchedDirChanged;
                logWatcher.EnableRaisingEvents = true;
            }
        }

        private void WatchedDirChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            OnGlobalLogFileChanged();
        }

        [StringFormatMethod("format")]
        public void AppendGlobalLineFormat(string format, params object[] args)
        {
            if (fileLock == null)
            {
                return;
            }
            
            if (fileLock.AcquireLockTimeout(5000, 500))
            {
                try
                {
                    using (var writer = File.AppendText(config.SharedLogFile))
                    {
                        writer.Write("{0:dd/MM/yyyy HH:mm:ss} : {1} : {2} : ", DateTime.Now, Environment.MachineName, Environment.UserName);
                        writer.WriteLine(format, args);
                    }
                }
                finally
                {
                    fileLock.ReleaseLock();
                }
            }
        }

        public event EventHandler GlobalLogFileChanged;

        protected virtual void OnGlobalLogFileChanged()
        {
            GlobalLogFileChanged?.Invoke(this, EventArgs.Empty);
        }

        public IEnumerable<string> GetLogTail(int? numberOfLines)
        {
            IEnumerable<string> logLines = null;
            if (fileLock.AcquireLockTimeout(5000, 500))
            {
                try
                {
                    bool readWholeFile;
                    logLines = IOUtils.ReadTail(config.SharedLogFile, numberOfLines, out readWholeFile);
                    if (!readWholeFile)
                    {
                        return new[] {"(excerpt ends here)"}.Concat(logLines);
                    }
                }
                finally
                {
                    fileLock.ReleaseLock();
                }
            }
            return logLines;
        }
    }
}