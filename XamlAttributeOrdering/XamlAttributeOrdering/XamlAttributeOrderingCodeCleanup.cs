using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using JetBrains.Application;
using JetBrains.Application.Progress;
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
                sourceFile.GetPsiServices().Transactions.Execute("Code cleanup", () =>
                {
                    var attributeGroups = new[]
                    {
                        new[] {"Key", "x:Key"},
                        new[] {"Name", "x:Name", "Title"},
                        new[] { "Grid.Row", "Grid.RowSpan", "Grid.Column", "Grid.ColumnSpan", "Canvas.Left", "Canvas.Top", "Canvas.Right", "Canvas.Bottom" },
                        new[] { "Width", "Height", "MinWidth", "MinHeight", "MaxWidth", "MaxHeight", "Margin" },
                        new[] { "HorizontalAlignment", "VerticalAlignment", "HorizontalContentAlignment", "VerticalContentAlignment", "Panel.ZIndex" },
                        new[] { "PageSource", "PageIndex", "Offset", "Color", "TargetName", "Property", "Value", "StartPoint", "EndPoint" }
                    };
                    xamlFile.ProcessDescendants(new RecursiveElementProcessor<IXmlTag>(t =>
                    {
                        var attributes = t.Header.Attributes.ToList();

                        using (WriteLockCookie.Create())
                        {
                            foreach (var attribute in attributes)
                            {
                                if (attribute is INamespaceAlias)
                                    continue;
                                t.RemoveAttribute(attribute);
                                Debug.WriteLine(attribute.GetType());
                            }

                            foreach (var attribute in attributes.OfType<IXClassAttribute>().ToArray())
                            {
                                t.AddAttributeBefore(attribute, null);
                                attributes.Remove(attribute);
                            }

                            // Leave namespaces as they are for the moment as there is something odd going on.
                            //foreach (var attribute in sorted.OfType<INamespaceAlias>())
                            //{
                            //    attribute.SetResolveContextForSandBox(t, SandBoxContextType.Child);
                            //    anchor = t.AddAttributeAfter(attribute, anchor);
                            //}

                            foreach (var attributeGroup in attributeGroups)
                            {
                                foreach (var attribute in SortedByPriority(attributes, attributeGroup).ToArray())
                                {
                                    t.AddAttributeBefore(attribute, null);
                                    attributes.Remove(attribute);
                                }
                            }

                            //REST
                            foreach (var attribute in attributes)
                                t.AddAttributeBefore(attribute, null);
                        }
                    }));
                });
            }
        }

        private IEnumerable<IXmlAttribute> SortedByPriority(IEnumerable<IXmlAttribute> attributes, string[] attributeNames)
        {
            return attributes.SelectMany(a =>
            {
                var index = Array.IndexOf(attributeNames, a.AttributeName);
                if (index < 0) return Enumerable.Empty<Tuple<IXmlAttribute, int>>();
                return new []{Tuple.Create(a, index)};
            })
            .OrderBy(a=>a.Item2)
            .Select(a=>a.Item1);
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