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

        // note: extra first line in .gold file is produced
        // by the 'CodeCleanupTestBase' infrastructure (for dumping something).
        // just ignore it, since in VS environment there is no extra line.

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
