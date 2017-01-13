using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if !NO_LOCK
using FileLock;
using JetBrains.Annotations;
#endif 
using Remote.Utils;

namespace Remote
{
    public class Logging
    {
        private readonly Config config;
        #if !NO_LOCK
        private readonly SimpleFileLock fileLock;
        #endif
        private readonly FileSystemWatcher logWatcher;

        public Logging(Config config)
        {
            this.config = config;
            if (!string.IsNullOrEmpty(config?.SharedLogFile))
            {
                #if !NO_LOCK
                fileLock = SimpleFileLock.Create(config.SharedLogFile + ".lock", TimeSpan.FromMinutes(1));
                #endif
                logWatcher = new FileSystemWatcher(Path.GetDirectoryName(config.SharedLogFile), Path.GetFileName(config.SharedLogFile));
                logWatcher.Changed += WatchedDirChanged;
                logWatcher.EnableRaisingEvents = true;
            }
        }

        private void WatchedDirChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            OnGlobalLogFileChanged();
        }

        public void AppendGlobalLine(string message)
        {
            #if !NO_LOCK
            if (fileLock == null)
            {
                return;
            }
            
            if (fileLock.AcquireLockTimeout(5000, 500))
            #endif
            {
                try
                {
                    using (var writer = File.AppendText(config.SharedLogFile))
                    {
                        writer.Write("{0:dd/MM/yyyy HH:mm:ss} : {1} : {2} : ", DateTime.Now, Environment.MachineName, Environment.UserName);
                        writer.WriteLine(message);
                    }
                }
                finally
                {
                    #if !NO_LOCK
                    fileLock.ReleaseLock();
                    #endif
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
            if (config.SharedLogFile != null && File.Exists(config.SharedLogFile)
                #if !NO_LOCK
                && fileLock.AcquireLockTimeout(5000, 500)
                #endif
                )
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
                    #if !NO_LOCK
                    fileLock.ReleaseLock();
                    #endif
                }
            }
            return logLines;
        }
    }
}