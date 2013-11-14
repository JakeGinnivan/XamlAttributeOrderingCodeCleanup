using JetBrains.ReSharper.FeaturesTestFramework.CodeCleanup;
using NUnit.Framework;

namespace XamlAttributeOrdering.Tests
{
    [TestFixture]
    public class CodeCleanupTest1 : CodeCleanupTestBase
    {
        protected override string RelativeTestDataPath
        {
            get { return ""; }
        }

        [Test]
        public void TestFullCleanup()
        {
            DoTestFiles("UserControl1.xaml");
        }

        [Test]
        public void TestWithoutCleanup()
        {
            DoTestFilesWithProfile("disableCleanup.profile", "UserControl1.xaml");
        }
    }
}
