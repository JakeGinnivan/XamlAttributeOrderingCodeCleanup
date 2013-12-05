using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.Application.Progress;
using JetBrains.Application.Settings;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.CodeCleanup;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Xaml;
using JetBrains.ReSharper.Psi.Xaml.Tree;
using JetBrains.ReSharper.Psi.Xml.Tree;

namespace XamlAttributeOrdering
{
    //(ModulesAfter = new[] { typeof(IXmlContextActionProvider).Assembly.GetType("JetBrains.ReSharper.Feature.Services.Xml.CodeCleanup.XmlReformatCleanupModule") })
    [CodeCleanupModule]
    public class XamlAttributeOrderingCodeCleanup : ICodeCleanupModule
    {
        private readonly ISettingsStore _settingsStore;
        private readonly DataContexts _dataContexts;
        private static readonly Descriptor DescriptorInstance = new Descriptor();

        public XamlAttributeOrderingCodeCleanup(ISettingsStore settingsStore, DataContexts dataContexts)
        {
            _settingsStore = settingsStore;
            _dataContexts = dataContexts;
        }

        public PsiLanguageType LanguageType
        {
            get { return XamlLanguage.Instance; }
        }

        public ICollection<CodeCleanupOptionDescriptor> Descriptors
        {
            get { return new CodeCleanupOptionDescriptor[] {DescriptorInstance}; }
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
            var settings = GetXamlAttributeOrderingSettings();
            return settings.Enable && sourceFile.GetPsiFiles<XamlLanguage>().Any();
        }

        public void Process(IPsiSourceFile sourceFile, IRangeMarker rangeMarker, CodeCleanupProfile profile,
            IProgressIndicator progressIndicator)
        {
            var settings = GetXamlAttributeOrderingSettings();
            var enabled = settings.Enable;
            if (!profile.GetSetting(DescriptorInstance) ||!enabled)
                return;

            IXamlFile[] xamlFileArray = sourceFile.GetPsiFiles<XamlLanguage>().Cast<IXamlFile>().ToArray();
            foreach (IXamlFile xamlFile in xamlFileArray)
            {
                LanguageService languageService = xamlFile.Language.LanguageService();
                if (languageService == null)
                    break;

                sourceFile.GetPsiServices().Transactions.Execute("Code cleanup", () =>
                {
                    var attributeGroups = new[]
                    {
                        settings.Group1_KeyGroup,
                        settings.Group2_NameGroup,
                        settings.Group3_AttachedLayoutGroup,
                        settings.Group4_LayoutGroup,
                        settings.Group5_AlignmentGroup,
                        settings.Group6_MiscGroup
                    };
                    xamlFile.ProcessDescendants(new RecursiveElementProcessor<IXmlTag>(t =>
                    {
                        List<IXmlAttribute> attributes = t.Header.Attributes.ToList();

                        using (WriteLockCookie.Create())
                        {
                            foreach (IXmlAttribute attribute in attributes)
                            {
                                if (attribute is INamespaceAlias)
                                    continue;
                                t.RemoveAttribute(attribute);
                            }

                            foreach (IXClassAttribute attribute in attributes.OfType<IXClassAttribute>().ToArray())
                            {
                                t.AddAttributeBefore(attribute, null);
                                attributes.Remove(attribute);
                            }

                            // Dont want to double up
                            foreach (INamespaceAlias attribute in attributes.OfType<INamespaceAlias>().ToArray())
                            {
                                attributes.Remove(attribute);
                            }

                            foreach (var attributeGroup in attributeGroups)
                            {
                                var attributeNames = attributeGroup.Split(',').Select(a => a.Trim()).ToArray();
                                foreach (var attribute in SortedByPriority(attributes, attributeNames).ToArray())
                                {
                                    t.AddAttributeBefore(attribute, null);
                                    attributes.Remove(attribute);
                                }
                            }

                            //REST
                            foreach (IXmlAttribute attribute in attributes)
                                t.AddAttributeBefore(attribute, null);
                        }
                    }));
                });
            }
        }

        private XamlAttributeOrderingSettings GetXamlAttributeOrderingSettings()
        {
            var boundSettings = _settingsStore.BindToContextTransient(ContextRange.Smart((lt, _) => _dataContexts.Empty));
            var settings = boundSettings.GetKey<XamlAttributeOrderingSettings>(SettingsOptimization.DoMeSlowly);
            return settings;
        }

        private IEnumerable<IXmlAttribute> SortedByPriority(IEnumerable<IXmlAttribute> attributes,
            string[] attributeNames)
        {
            return attributes.SelectMany(a =>
            {
                int index = Array.IndexOf(attributeNames, a.AttributeName);
                if (index < 0) return Enumerable.Empty<Tuple<IXmlAttribute, int>>();
                return new[] {Tuple.Create(a, index)};
            })
                .OrderBy(a => a.Item2)
                .Select(a => a.Item1);
        }

        [DefaultValue(true)]
        [DisplayName("Reorder Xaml Attributes")]
        [Category(XamlCategory)]
        private class Descriptor : CodeCleanupBoolOptionDescriptor
        {
            public Descriptor()
                : base("ReorderXamlAttributes")
            {
            }
        }
    }
}