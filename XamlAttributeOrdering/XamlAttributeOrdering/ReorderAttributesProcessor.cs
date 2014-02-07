using System.Collections.Generic;
using System.Linq;

using JetBrains.Application;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Xaml.Impl;
using JetBrains.ReSharper.Psi.Xml.Tree;

namespace XamlAttributeOrdering {
    internal class ReorderAttributesProcessor : IRecursiveElementProcessor {
        private readonly IComparer<IXmlAttribute> _orderComparer;

        public ReorderAttributesProcessor(IComparer<IXmlAttribute> orderComparer) {
            _orderComparer = orderComparer;
        }

        public bool ProcessingIsFinished {
            get { return false; }
        }

        public bool InteriorShouldBeProcessed(ITreeNode element) {
            return !(element is IXmlTagHeader)
                   && !(element is IXmlTagFooter);
        }

        public void ProcessAfterInterior(ITreeNode element) {
        }

        public void ProcessBeforeInterior(ITreeNode element) {
            var header = element as IXmlTagHeader;
            if (header == null) {
                return;
            }

            // note: using LINQ's .OrderBy() because of stable sorting behavior
            var sortedAttributes = header.Attributes.OrderBy(x => x, _orderComparer).ToList();
            if (sortedAttributes.SequenceEqual(header.AttributesEnumerable)) {
                return;
            }

            using (WriteLockCookie.Create()) {
                var xamlFactory = XamlElementFactory.GetInstance(header);
                var replacementMap = new Dictionary<IXmlAttribute, IXmlAttribute>();

                // I'm using LowLevelModificationUtil to physically modify AST at low-level,
                // without cloning, invoking reference binds and formatting
                // (like ModificationUtil.*/.RemoveAttribute()/.AddAttributeBefore() methods do).

                var attributes = header.Attributes;
                for (var index = 0; index < attributes.Count; index++) {
                    var attribute = attributes[index];
                    var sortedAttribute = sortedAttributes[index];
                    if (attribute == sortedAttribute) {
                        continue;
                    }

                    // replace attribute to be reordered with fake attribute
                    var fakeAttribute = xamlFactory.CreateRootAttribute("fake");
                    LowLevelModificationUtil.ReplaceChildRange(attribute, attribute, fakeAttribute);
                    replacementMap.Add(fakeAttribute, sortedAttribute);
                }

                // now all attributes in 'replacementMap' are detached from AST and replaces with fake ones
                // let's now replace fakes with attributes in order:

                foreach (var attribute in replacementMap) {
                    var fakeAttribute = attribute.Key;
                    LowLevelModificationUtil.ReplaceChildRange(fakeAttribute, fakeAttribute, attribute.Value);
                }
            }
        }
    }
}