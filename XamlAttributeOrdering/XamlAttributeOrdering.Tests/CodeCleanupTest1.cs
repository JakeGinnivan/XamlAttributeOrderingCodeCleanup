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
        public void TestFullCleanup2()
        {
            DoTestFiles("UserControl1.xaml");
        }

        [Test]
        public void TestWithoutCleanup2()
        {
            DoTestFilesWithProfile("disableCleanup.profile", "UserControl1.xaml");
        }
    }
}
