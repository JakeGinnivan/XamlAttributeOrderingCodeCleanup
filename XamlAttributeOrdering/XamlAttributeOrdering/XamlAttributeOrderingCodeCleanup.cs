using System.Collections.Generic;
using System.Linq;

using JetBrains.Application.Progress;
using JetBrains.Application.Settings;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.CodeCleanup;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Xaml;
using JetBrains.ReSharper.Psi.Xaml.Tree;

namespace XamlAttributeOrdering {
    //(ModulesAfter = new[] { typeof(IXmlContextActionProvider).Assembly.GetType("JetBrains.ReSharper.Feature.Services.Xml.CodeCleanup.XmlReformatCleanupModule") })
    [CodeCleanupModule]
    public class XamlAttributeOrderingCodeCleanup : ICodeCleanupModule {
        private static readonly Descriptor DescriptorInstance = new Descriptor();

        public ICollection<CodeCleanupOptionDescriptor> Descriptors {
            get { return new CodeCleanupOptionDescriptor[] { DescriptorInstance }; }
        }

        public bool IsAvailableOnSelection {
            get { return false; }
        }
        public PsiLanguageType LanguageType {
            get { return XamlLanguage.Instance; }
        }

        public bool IsAvailable(IPsiSourceFile sourceFile) {
            var settingsStore = sourceFile.GetSettingsStore();
            return settingsStore.GetValue((XamlAttributeOrderingSettings x) => x.Enable)
                   && sourceFile.GetPsiFiles<XamlLanguage>().Any();
        }

        public void Process(
                IPsiSourceFile sourceFile, IRangeMarker rangeMarker,
                CodeCleanupProfile profile, IProgressIndicator progressIndicator) {
            var settingsStore = sourceFile.GetSettingsStore();
            var settings = settingsStore.GetKey<XamlAttributeOrderingSettings>(SettingsOptimization.OptimizeDefault);
            if (!profile.GetSetting(DescriptorInstance) || !settings.Enable) {
                return;
            }

            foreach (var xamlFile in sourceFile.GetPsiFiles<XamlLanguage>().OfType<IXamlFile>()) {
                sourceFile.GetPsiServices().Transactions.Execute("Code cleanup",
                        () => {
                            var comparer = new AttributesComparer(settings);
                            xamlFile.ProcessDescendants(new ReorderAttributesProcessor(comparer));
                        });
            }
        }

        public void SetDefaultSetting(CodeCleanupProfile profile, CodeCleanup.DefaultProfileType profileType) {
        }
    }
}