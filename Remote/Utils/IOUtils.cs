using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
#if !NO_LOCK
using FileLock;
#endif

namespace Remote.Utils
{
    public static class IOUtils
    {
        public static Regex WildcardToRegex(string wildcard)
        {
            if (string.IsNullOrEmpty(wildcard))
                return null;
            if (!wildcard.StartsWith("*"))
                wildcard = "*" + wildcard;
            if (!wildcard.EndsWith("*"))
                wildcard = wildcard + "*";
            var pattern = "^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex;
        }

        public static IEnumerable<string> ReadTail(string path, long? numberOfLines, out bool readWholeFile)
        {
            if (!numberOfLines.HasValue)
            {
                readWholeFile = true;
                return File.ReadAllLines(path);
            }

            Encoding encoding = Encoding.ASCII;
            string tokenSeparator = Environment.NewLine;
            int sizeOfChar = encoding.GetByteCount("\n");
            byte[] buffer = encoding.GetBytes(tokenSeparator);
            List<string> result = new List<string>();
            readWholeFile = true;

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                long tokenCount = 0;
                long endPosition = fs.Length / sizeOfChar;
                long lastPosition = fs.Length;

                for (var position = sizeOfChar; position < endPosition; position += sizeOfChar)
                {
                    fs.Seek(-position, SeekOrigin.End);
                    fs.Read(buffer, 0, buffer.Length);

                    if (encoding.GetString(buffer) == tokenSeparator)
                    {
                        {
                            byte[] returnBuffer = new byte[lastPosition - fs.Position];
                            lastPosition = fs.Position;
                            fs.Read(returnBuffer, 0, returnBuffer.Length);
                            string line = encoding.GetString(returnBuffer).Trim();
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                result.Add(line);
                                tokenCount++;
                            }
                        }
                        if (numberOfLines.HasValue && tokenCount == numberOfLines.Value)
                        {
                            readWholeFile = false;
                            break;
                        }
                    }
                }
            }
            result.Reverse();
            return result;
        }

        public static bool RetryUntilSuccessOrTimeout(Func<bool> task, TimeSpan timeout, TimeSpan pause)
        {
            if (pause.TotalMilliseconds < 0)
            {
                throw new ArgumentException("pause must be >= 0 milliseconds");
            }
            var stopwatch = Stopwatch.StartNew();
            do
            {
                if (task()) { return true; }
                Thread.Sleep((int)pause.TotalMilliseconds);
            }
            while (stopwatch.Elapsed < timeout);
            return false;
        }

        #if !NO_LOCK
        public static bool AcquireLockTimeout(this SimpleFileLock fileLock, int timeoutMs, int pauseMs )
        {
            return RetryUntilSuccessOrTimeout(fileLock.TryAcquireLock,
                TimeSpan.FromMilliseconds(timeoutMs), TimeSpan.FromMilliseconds(pauseMs));

        }
        #endif
    }
}