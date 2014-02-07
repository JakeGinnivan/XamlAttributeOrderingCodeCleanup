using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using JetBrains.ReSharper.Psi.Xaml.Tree;
using JetBrains.ReSharper.Psi.Xml.Tree;

namespace XamlAttributeOrdering {
    internal class AttributesComparer : IComparer<IXmlAttribute> {
        private readonly Dictionary<string, int> _nameWeights;
        private bool _isAlphabeticalOrder;

        public AttributesComparer(XamlAttributeOrderingSettings settings) {
            if (settings.IsAlphabeticalOrder) {
                _isAlphabeticalOrder = true;
            } else {
                var attributeGroups = new[] {
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
                        .SelectMany(group => @group.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        .Select(s => s.Trim())
                        .Where(s => !String.IsNullOrWhiteSpace(s)))
                {
                    // todo: prevent from/warn about duplicate names in settings
                    if (!nameWeights.ContainsKey(name))
                    {
                        nameWeights.Add(name, weight++);
                    }
                }

                _nameWeights = nameWeights;
            }
        }

        public int Compare(IXmlAttribute x, IXmlAttribute y) {
            if (_isAlphabeticalOrder) {
                return String.Compare(x.AttributeName, y.AttributeName, StringComparison.Ordinal);
            } else {
                var xWeight = WeightAttribute(x);
                var yWeight = WeightAttribute(y);

                // todo: you can add alphabetical sort here for weights == int.MaxValue

                return xWeight.CompareTo(yWeight);
            }
        }

        private int WeightAttribute(IXmlAttribute attribute) {
            if (attribute is IXClassAttribute) {
                return -2;
            }
            if (attribute is INamespaceAlias) {
                return -1;
            }

            int value;
            if (_nameWeights.TryGetValue(attribute.AttributeName, out value)) {
                return value;
            }

            return Int32.MaxValue;
        }
    }
}