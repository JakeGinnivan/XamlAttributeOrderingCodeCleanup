using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Application;
using JetBrains.Application.Progress;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.CodeCleanup;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Xaml;
using JetBrains.ReSharper.Psi.Xaml.Tree;
using JetBrains.ReSharper.Psi.Xml.Impl.Tree;
using JetBrains.ReSharper.Psi.Xml.Tree;

namespace XamlAttributeOrdering
{
    [CodeCleanupModule]
    public class XamlAttributeOrderingCodeCleanup : ICodeCleanupModule
    {
        private static readonly Descriptor DescriptorInstance = new Descriptor();

        public PsiLanguageType LanguageType
        {
            get { return XamlLanguage.Instance; }
        }

        public ICollection<CodeCleanupOptionDescriptor> Descriptors
        {
            get { return new CodeCleanupOptionDescriptor[] { DescriptorInstance }; }
        }

        public bool IsAvailableOnSelection
        {
            get { return false; }
        }

        public void SetDefaultSetting(CodeCleanupProfile profile, CodeCleanup.DefaultProfileType profileType)
        {
        }

        public bool IsAvailable(IPsiSourceFile sourceFile)
        {
            return sourceFile.GetPsiFiles<XamlLanguage>().Any();
        }

        public void Process(IPsiSourceFile sourceFile, IRangeMarker rangeMarker, CodeCleanupProfile profile,
            IProgressIndicator progressIndicator)
        {
            if (!profile.GetSetting(DescriptorInstance))
                return;

            var xamlFileArray = sourceFile.GetPsiFiles<XamlLanguage>().Cast<IXamlFile>().ToArray();
            foreach (var xamlFile in xamlFileArray)
            {
                LanguageService languageService = xamlFile.Language.LanguageService();
                if (languageService == null)
                    break;
                var xamlElementTypes = XmlElementTypes.GetInstance<XamlElementTypes>(XamlLanguage.Instance);
                var xmlCompositeNodeType = xamlElementTypes.TAG_HEADER;
                sourceFile.GetPsiServices().Transactions.Execute("Code cleanup", () =>
                {
                    using (WriteLockCookie.Create())
                    {
                        xamlFile.ProcessDescendants(new RecursiveElementProcessor(t =>
                        {
                            if (t.NodeType == xmlCompositeNodeType)
                            {
                                var container = (XmlTagHeaderNode)t;
                                
                                var attributes = container.Children<IXmlAttribute>().ToArray();

                                foreach (var xmlAttribute in attributes)
                                {
                                    xmlAttribute.Remove();
                                }

                                //HOW do i make this work..
                                foreach (var xmlAttribute in attributes.OrderBy(a=>a.AttributeName))
                                {
                                    container.AddAttributeBefore(xmlAttribute, null);
                                }
                            }
                        }));
                    }
                });
            }
        }

        [DefaultValue(true)]
        [DisplayName("Format Xaml Attributes")]
        [Category(XamlCategory)]
        private class Descriptor : CodeCleanupBoolOptionDescriptor
        {
            public Descriptor()
                : base("FormatXamlAttributes")
            {
            }
        }
    }
}