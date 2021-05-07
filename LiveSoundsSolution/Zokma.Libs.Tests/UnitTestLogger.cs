using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Zokma.Libs.Logging;

namespace Zokma.Libs.Tests
{
    public class UnitTestLogger
    {
        private static readonly Pathfinder logDir = new Pathfinder("log");
        private readonly ITestOutputHelper output;

        public UnitTestLogger(ITestOutputHelper output)
        {
            Log.Init(logDir.FindPathName("test.log"));

            this.output = output;
        }

        private void Func1()
        {
            Func2();
        }

        private void Func2()
        {
            throw new NotImplementedException("Exception at Func2().");
        }

        private static void WriteLog(LogLevel level)
        {
            Log.LogLevel = level;

            Log.Verbose("Verbose level={Level}", level);
            Log.Debug("Debug level={Level}", level);
            Log.Information("Information level={Level}", level);
            Log.Warning("Warning level={Level}", level);
            Log.Error("Error level={Level}", level);
            Log.Fatal("Fatal level={Level}", level);
        }

        private static void CheckLogLevel(LogLevel level)
        {
            Log.LogLevel = level;

            if(level == LogLevel.Verbose)
            {
                Assert.True(Log.IsEnabled(LogLevel.Verbose));
                Assert.True(Log.IsEnabled(LogLevel.Debug));
                Assert.True(Log.IsEnabled(LogLevel.Information));
                Assert.True(Log.IsEnabled(LogLevel.Warning));
                Assert.True(Log.IsEnabled(LogLevel.Error));
                Assert.True(Log.IsEnabled(LogLevel.Fatal));

                Assert.True(Log.IsVerboseEnabled);
                Assert.True(Log.IsDebugEnabled);
                Assert.True(Log.IsInformationEnabled);
                Assert.True(Log.IsWarningEnabled);
                Assert.True(Log.IsErrorEnabled);
                Assert.True(Log.IsFatalEnabled);

                Assert.False(Log.IsEnabled(LogLevel.Silent));
                Assert.False(Log.IsEnabled(LogLevel.None));
            }
            else if(level == LogLevel.Debug)
            {
                Assert.False(Log.IsEnabled(LogLevel.Verbose));
                Assert.True(Log.IsEnabled(LogLevel.Debug));
                Assert.True(Log.IsEnabled(LogLevel.Information));
                Assert.True(Log.IsEnabled(LogLevel.Warning));
                Assert.True(Log.IsEnabled(LogLevel.Error));
                Assert.True(Log.IsEnabled(LogLevel.Fatal));

                Assert.False(Log.IsVerboseEnabled);
                Assert.True(Log.IsDebugEnabled);
                Assert.True(Log.IsInformationEnabled);
                Assert.True(Log.IsWarningEnabled);
                Assert.True(Log.IsErrorEnabled);
                Assert.True(Log.IsFatalEnabled);

                Assert.False(Log.IsEnabled(LogLevel.Silent));
                Assert.False(Log.IsEnabled(LogLevel.None));
            }
            else if (level == LogLevel.Information)
            {
                Assert.False(Log.IsEnabled(LogLevel.Verbose));
                Assert.False(Log.IsEnabled(LogLevel.Debug));
                Assert.True(Log.IsEnabled(LogLevel.Information));
                Assert.True(Log.IsEnabled(LogLevel.Warning));
                Assert.True(Log.IsEnabled(LogLevel.Error));
                Assert.True(Log.IsEnabled(LogLevel.Fatal));

                Assert.False(Log.IsVerboseEnabled);
                Assert.False(Log.IsDebugEnabled);
                Assert.True(Log.IsInformationEnabled);
                Assert.True(Log.IsWarningEnabled);
                Assert.True(Log.IsErrorEnabled);
                Assert.True(Log.IsFatalEnabled);

                Assert.False(Log.IsEnabled(LogLevel.Silent));
                Assert.False(Log.IsEnabled(LogLevel.None));
            }
            else if (level == LogLevel.Warning)
            {
                Assert.False(Log.IsEnabled(LogLevel.Verbose));
                Assert.False(Log.IsEnabled(LogLevel.Debug));
                Assert.False(Log.IsEnabled(LogLevel.Information));
                Assert.True(Log.IsEnabled(LogLevel.Warning));
                Assert.True(Log.IsEnabled(LogLevel.Error));
                Assert.True(Log.IsEnabled(LogLevel.Fatal));

                Assert.False(Log.IsVerboseEnabled);
                Assert.False(Log.IsDebugEnabled);
                Assert.False(Log.IsInformationEnabled);
                Assert.True(Log.IsWarningEnabled);
                Assert.True(Log.IsErrorEnabled);
                Assert.True(Log.IsFatalEnabled);

                Assert.False(Log.IsEnabled(LogLevel.Silent));
                Assert.False(Log.IsEnabled(LogLevel.None));
            }
            else if (level == LogLevel.Error)
            {
                Assert.False(Log.IsEnabled(LogLevel.Verbose));
                Assert.False(Log.IsEnabled(LogLevel.Debug));
                Assert.False(Log.IsEnabled(LogLevel.Information));
                Assert.False(Log.IsEnabled(LogLevel.Warning));
                Assert.True(Log.IsEnabled(LogLevel.Error));
                Assert.True(Log.IsEnabled(LogLevel.Fatal));

                Assert.False(Log.IsVerboseEnabled);
                Assert.False(Log.IsDebugEnabled);
                Assert.False(Log.IsInformationEnabled);
                Assert.False(Log.IsWarningEnabled);
                Assert.True(Log.IsErrorEnabled);
                Assert.True(Log.IsFatalEnabled);

                Assert.False(Log.IsEnabled(LogLevel.Silent));
                Assert.False(Log.IsEnabled(LogLevel.None));
            }
            else if (level == LogLevel.Fatal)
            {
                Assert.False(Log.IsEnabled(LogLevel.Verbose));
                Assert.False(Log.IsEnabled(LogLevel.Debug));
                Assert.False(Log.IsEnabled(LogLevel.Information));
                Assert.False(Log.IsEnabled(LogLevel.Warning));
                Assert.False(Log.IsEnabled(LogLevel.Error));
                Assert.True(Log.IsEnabled(LogLevel.Fatal));

                Assert.False(Log.IsVerboseEnabled);
                Assert.False(Log.IsDebugEnabled);
                Assert.False(Log.IsInformationEnabled);
                Assert.False(Log.IsWarningEnabled);
                Assert.False(Log.IsErrorEnabled);
                Assert.True(Log.IsFatalEnabled);

                Assert.False(Log.IsEnabled(LogLevel.Silent));
                Assert.False(Log.IsEnabled(LogLevel.None));
            }
            else if (level == LogLevel.Silent)
            {
                Assert.False(Log.IsEnabled(LogLevel.Verbose));
                Assert.False(Log.IsEnabled(LogLevel.Debug));
                Assert.False(Log.IsEnabled(LogLevel.Information));
                Assert.False(Log.IsEnabled(LogLevel.Warning));
                Assert.False(Log.IsEnabled(LogLevel.Error));
                Assert.False(Log.IsEnabled(LogLevel.Fatal));

                Assert.False(Log.IsVerboseEnabled);
                Assert.False(Log.IsDebugEnabled);
                Assert.False(Log.IsInformationEnabled);
                Assert.False(Log.IsWarningEnabled);
                Assert.False(Log.IsErrorEnabled);
                Assert.False(Log.IsFatalEnabled);

                Assert.False(Log.IsEnabled(LogLevel.Silent));
                Assert.False(Log.IsEnabled(LogLevel.None));
            }
            else if (level == LogLevel.None)
            {
                Assert.False(Log.IsEnabled(LogLevel.Verbose));
                Assert.False(Log.IsEnabled(LogLevel.Debug));
                Assert.False(Log.IsEnabled(LogLevel.Information));
                Assert.False(Log.IsEnabled(LogLevel.Warning));
                Assert.False(Log.IsEnabled(LogLevel.Error));
                Assert.False(Log.IsEnabled(LogLevel.Fatal));

                Assert.False(Log.IsVerboseEnabled);
                Assert.False(Log.IsDebugEnabled);
                Assert.False(Log.IsInformationEnabled);
                Assert.False(Log.IsWarningEnabled);
                Assert.False(Log.IsErrorEnabled);
                Assert.False(Log.IsFatalEnabled);

                Assert.False(Log.IsEnabled(LogLevel.Silent));
                Assert.False(Log.IsEnabled(LogLevel.None));
            }


        }

        [Fact]
        public void TestWriteLog()
        {
            var dict = new Dictionary<string, string>
            {
                { "key1", "val1" },
                { "key2", "val2" },
                { "key3", "val3" },
            };

            var exception = new StackOverflowException("Exception message.");

            Log.LogLevel = LogLevel.Verbose;

            Log.Verbose("Message.");
            Log.Verbose("Message Dict={Val1}.", dict);
            Log.Verbose("Message Dict={Val1}, Num={Val2}.", dict, 1234);
            Log.Verbose("Message Dict={Val1}, Num={Val2}, Str={Val3}.", dict, 1234, "Hello");
            Log.Verbose("Message Dict={Val1}, Num={Val2}, Str={Val3}, Bool={Val4}.", dict, 1234, "Hello", true);
            Log.Verbose(exception, "Message.");
            Log.Verbose(exception, "Message Dict={Val1}.", dict);
            Log.Verbose(exception, "Message Dict={Val1}, Num={Val2}.", dict, 1234);
            Log.Verbose(exception, "Message Dict={Val1}, Num={Val2}, Str={Val3}.", dict, 1234, "Hello");
            Log.Verbose(exception, "Message Dict={Val1}, Num={Val2}, Str={Val3}, Bool={Val4}.", dict, 1234, "Hello", true);

            Log.Debug("Message.");
            Log.Debug("Message Dict={Val1}.", dict);
            Log.Debug("Message Dict={Val1}, Num={Val2}.", dict, 1234);
            Log.Debug("Message Dict={Val1}, Num={Val2}, Str={Val3}.", dict, 1234, "Hello");
            Log.Debug("Message Dict={Val1}, Num={Val2}, Str={Val3}, Bool={Val4}.", dict, 1234, "Hello", true);
            Log.Debug(exception, "Message.");
            Log.Debug(exception, "Message Dict={Val1}.", dict);
            Log.Debug(exception, "Message Dict={Val1}, Num={Val2}.", dict, 1234);
            Log.Debug(exception, "Message Dict={Val1}, Num={Val2}, Str={Val3}.", dict, 1234, "Hello");
            Log.Debug(exception, "Message Dict={Val1}, Num={Val2}, Str={Val3}, Bool={Val4}.", dict, 1234, "Hello", true);

            Log.Information("Message.");
            Log.Information("Message Dict={Val1}.", dict);
            Log.Information("Message Dict={Val1}, Num={Val2}.", dict, 1234);
            Log.Information("Message Dict={Val1}, Num={Val2}, Str={Val3}.", dict, 1234, "Hello");
            Log.Information("Message Dict={Val1}, Num={Val2}, Str={Val3}, Bool={Val4}.", dict, 1234, "Hello", true);
            Log.Information(exception, "Message.");
            Log.Information(exception, "Message Dict={Val1}.", dict);
            Log.Information(exception, "Message Dict={Val1}, Num={Val2}.", dict, 1234);
            Log.Information(exception, "Message Dict={Val1}, Num={Val2}, Str={Val3}.", dict, 1234, "Hello");
            Log.Information(exception, "Message Dict={Val1}, Num={Val2}, Str={Val3}, Bool={Val4}.", dict, 1234, "Hello", true);

            Log.Warning("Message.");
            Log.Warning("Message Dict={Val1}.", dict);
            Log.Warning("Message Dict={Val1}, Num={Val2}.", dict, 1234);
            Log.Warning("Message Dict={Val1}, Num={Val2}, Str={Val3}.", dict, 1234, "Hello");
            Log.Warning("Message Dict={Val1}, Num={Val2}, Str={Val3}, Bool={Val4}.", dict, 1234, "Hello", true);
            Log.Warning(exception, "Message.");
            Log.Warning(exception, "Message Dict={Val1}.", dict);
            Log.Warning(exception, "Message Dict={Val1}, Num={Val2}.", dict, 1234);
            Log.Warning(exception, "Message Dict={Val1}, Num={Val2}, Str={Val3}.", dict, 1234, "Hello");
            Log.Warning(exception, "Message Dict={Val1}, Num={Val2}, Str={Val3}, Bool={Val4}.", dict, 1234, "Hello", true);

            Log.Error("Message.");
            Log.Error("Message Dict={Val1}.", dict);
            Log.Error("Message Dict={Val1}, Num={Val2}.", dict, 1234);
            Log.Error("Message Dict={Val1}, Num={Val2}, Str={Val3}.", dict, 1234, "Hello");
            Log.Error("Message Dict={Val1}, Num={Val2}, Str={Val3}, Bool={Val4}.", dict, 1234, "Hello", true);
            Log.Error(exception, "Message.");
            Log.Error(exception, "Message Dict={Val1}.", dict);
            Log.Error(exception, "Message Dict={Val1}, Num={Val2}.", dict, 1234);
            Log.Error(exception, "Message Dict={Val1}, Num={Val2}, Str={Val3}.", dict, 1234, "Hello");
            Log.Error(exception, "Message Dict={Val1}, Num={Val2}, Str={Val3}, Bool={Val4}.", dict, 1234, "Hello", true);

            Log.Fatal("Message.");
            Log.Fatal("Message Dict={Val1}.", dict);
            Log.Fatal("Message Dict={Val1}, Num={Val2}.", dict, 1234);
            Log.Fatal("Message Dict={Val1}, Num={Val2}, Str={Val3}.", dict, 1234, "Hello");
            Log.Fatal("Message Dict={Val1}, Num={Val2}, Str={Val3}, Bool={Val4}.", dict, 1234, "Hello", true);
            Log.Fatal(exception, "Message.");
            Log.Fatal(exception, "Message Dict={Val1}.", dict);
            Log.Fatal(exception, "Message Dict={Val1}, Num={Val2}.", dict, 1234);
            Log.Fatal(exception, "Message Dict={Val1}, Num={Val2}, Str={Val3}.", dict, 1234, "Hello");
            Log.Fatal(exception, "Message Dict={Val1}, Num={Val2}, Str={Val3}, Bool={Val4}.", dict, 1234, "Hello", true);

            try
            {
                Func1();
            }
            catch (Exception ex)
            {
                // Test for stacktrace.
                Log.Verbose(ex, "Exception.");
                Log.Debug(ex, "Exception.");
                Log.Information(ex, "Exception.");
                Log.Warning(ex, "Exception.");
                Log.Error(ex, "Exception.");
                Log.Fatal(ex, "Exception.");
            }

            WriteLog(LogLevel.Verbose);
            WriteLog(LogLevel.Debug);
            WriteLog(LogLevel.Information);
            WriteLog(LogLevel.Warning);
            WriteLog(LogLevel.Error);
            WriteLog(LogLevel.Fatal);

            CheckLogLevel(LogLevel.Verbose);
            CheckLogLevel(LogLevel.Debug);
            CheckLogLevel(LogLevel.Information);
            CheckLogLevel(LogLevel.Warning);
            CheckLogLevel(LogLevel.Error);
            CheckLogLevel(LogLevel.Fatal);
            CheckLogLevel(LogLevel.Silent);
            CheckLogLevel(LogLevel.None);

            Log.LogLevel = LogLevel.Verbose;
            Log.Information("Logger will be closed.");

            Log.Close();
            Log.Verbose("This message will not be output.");
            Log.Debug("This message will not be output.");
            Log.Information("This message will not be output.");
            Log.Warning("This message will not be output.");
            Log.Error("This message will not be output.");
            Log.Fatal("This message will not be output.");

            Assert.False(Log.IsEnabled(LogLevel.Verbose));
            Assert.False(Log.IsEnabled(LogLevel.Debug));
            Assert.False(Log.IsEnabled(LogLevel.Information));
            Assert.False(Log.IsEnabled(LogLevel.Warning));
            Assert.False(Log.IsEnabled(LogLevel.Error));
            Assert.False(Log.IsEnabled(LogLevel.Fatal));
            Assert.False(Log.IsEnabled(LogLevel.Silent));
            Assert.False(Log.IsEnabled(LogLevel.None));

            output.WriteLine("Please check log files at: {0}", logDir.FindPathName(String.Empty));
        }
    }
}
