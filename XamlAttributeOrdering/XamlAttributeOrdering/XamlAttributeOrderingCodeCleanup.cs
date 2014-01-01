using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Application;
using JetBrains.Application.Progress;
using JetBrains.Application.Settings;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.CodeCleanup;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Xaml;
using JetBrains.ReSharper.Psi.Xaml.Impl;
using JetBrains.ReSharper.Psi.Xaml.Tree;
using JetBrains.ReSharper.Psi.Xml.Tree;

namespace XamlAttributeOrdering
{
    //(ModulesAfter = new[] { typeof(IXmlContextActionProvider).Assembly.GetType("JetBrains.ReSharper.Feature.Services.Xml.CodeCleanup.XmlReformatCleanupModule") })
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
            var settingsStore = sourceFile.GetSettingsStore();
            return settingsStore.GetValue((XamlAttributeOrderingSettings x) => x.Enable)
                && sourceFile.GetPsiFiles<XamlLanguage>().Any();
        }

        public void Process(
            IPsiSourceFile sourceFile, IRangeMarker rangeMarker,
            CodeCleanupProfile profile, IProgressIndicator progressIndicator)
        {
            var settingsStore = sourceFile.GetSettingsStore();
            var settings = settingsStore.GetKey<XamlAttributeOrderingSettings>(SettingsOptimization.OptimizeDefault);
            if (!profile.GetSetting(DescriptorInstance) || !settings.Enable)
                return;

            foreach (var xamlFile in sourceFile.GetPsiFiles<XamlLanguage>().OfType<IXamlFile>())
            {
                sourceFile.GetPsiServices().Transactions.Execute("Code cleanup", () =>
                {
                    var comparer = new AttributesComparer(settings);
                    xamlFile.ProcessDescendants(new ReorderAttributesProcessor(comparer));
                });
            }
        }

        // todo: extract to outer scope?
        private class ReorderAttributesProcessor : IRecursiveElementProcessor
        {
            private readonly IComparer<IXmlAttribute> _orderComparer;

            public ReorderAttributesProcessor(IComparer<IXmlAttribute> orderComparer)
            {
                _orderComparer = orderComparer;
            }

            public bool InteriorShouldBeProcessed(ITreeNode element)
            {
                return !(element is IXmlTagHeader)
                    && !(element is IXmlTagFooter);
            }

            public void ProcessBeforeInterior(ITreeNode element)
            {
                var header = element as IXmlTagHeader;
                if (header == null)
                    return;

                // note: using LINQ's .OrderBy() because of stable sorting behavior
                var sortedAttributes = header.Attributes.OrderBy(x => x, _orderComparer).ToList();
                if (sortedAttributes.SequenceEqual(header.AttributesEnumerable))
                    return;

                using (WriteLockCookie.Create())
                {
                    var xamlFactory = XamlElementFactory.GetInstance(header);
                    var replacementMap = new Dictionary<IXmlAttribute, IXmlAttribute>();

                    // I'm using LowLevelModificationUtil to physically modify AST at low-level,
                    // without cloning, invoking reference binds and formatting
                    // (like ModificationUtil.*/.RemoveAttribute()/.AddAttributeBefore() methods do).

                    var attributes = header.Attributes;
                    for (var index = 0; index < attributes.Count; index++)
                    {
                        var attribute = attributes[index];
                        var sortedAttribute = sortedAttributes[index];
                        if (attribute == sortedAttribute) continue;

                        // replace attribute to be reordered with fake attribute
                        var fakeAttribute = xamlFactory.CreateRootAttribute("fake");
                        LowLevelModificationUtil.ReplaceChildRange(attribute, attribute, fakeAttribute);
                        replacementMap.Add(fakeAttribute, sortedAttribute);
                    }

                    // now all attributes in 'replacementMap' are detached from AST and replaces with fake ones
                    // let's now replace fakes with attributes in order:

                    foreach (var attribute in replacementMap)
                    {
                        var fakeAttribute = attribute.Key;
                        LowLevelModificationUtil.ReplaceChildRange(fakeAttribute, fakeAttribute, attribute.Value);
                    }
                }
            }

            public void ProcessAfterInterior(ITreeNode element) { }
            public bool ProcessingIsFinished { get { return false; } }
        }

        private class AttributesComparer : IComparer<IXmlAttribute>
        {
            private readonly Dictionary<string, int> _nameWeights;

            public AttributesComparer(XamlAttributeOrderingSettings settings)
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

                var weight = 0;
                var nameWeights = new Dictionary<string, int>(StringComparer.Ordinal);

                // flatten all the names and assign them corresponding weights
                foreach (var name in attributeGroups
                    .SelectMany(group => @group.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries))
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s)))
                {
                    // todo: prevent from/warn about duplicate names in settings
                    if (!nameWeights.ContainsKey(name))
                    {
                        nameWeights.Add(name, weight++);
                    }
                }

                _nameWeights = nameWeights;
            }

            private int WeightAttribute(IXmlAttribute attribute)
            {
                if (attribute is IXClassAttribute) return -2;
                if (attribute is INamespaceAlias) return -1;

                int value;
                if (_nameWeights.TryGetValue(attribute.AttributeName, out value))
                    return value;

                return int.MaxValue;
            }

            public int Compare(IXmlAttribute x, IXmlAttribute y)
            {
                var xWeight = WeightAttribute(x);
                var yWeight = WeightAttribute(y);

                // todo: you can add alphabetical sort here for weights == int.MaxValue

                return xWeight.CompareTo(yWeight);
            }
        }

        [DefaultValue(true)]
        [DisplayName("Reorder XAML Attributes")]
        [Category(XamlCategory)]
        private class Descriptor : CodeCleanupBoolOptionDescriptor
        {
            public Descriptor() : base("ReorderXamlAttributes") { }
        }
    }
}