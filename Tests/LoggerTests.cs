using Logger;

namespace Tests
{
    internal class LoggerTests
    {
        [Test]
        public void LoggerTest()
        {
            var consoleOrig = Console.Out;

            string actual = "";
            using (StringWriter stringWriter = new StringWriter())
            {
                LoggerSettings settings = new LoggerSettings()
                {
                    showDate = false,
                    showTime = false,
                    showType = false,
                };
                AsyncLogger logger = new AsyncLogger(settings, stringWriter);

                logger.Log("First Message", MES_TYPE.Info);
                Thread.Sleep(1000);
                logger.Log("Seccond Message", MES_TYPE.Info);
                logger.Log("Third Message", MES_TYPE.Info);
                Thread.Sleep(100);
                actual = stringWriter.ToString();
                Console.SetOut(consoleOrig);
            }
            string expected = "";
            expected += "First Message" + Environment.NewLine;
            expected += "Seccond Message" + Environment.NewLine;
            expected += "Third Message" + Environment.NewLine;

            Assert.That(actual, Is.EqualTo(expected));

        }

    }
}
