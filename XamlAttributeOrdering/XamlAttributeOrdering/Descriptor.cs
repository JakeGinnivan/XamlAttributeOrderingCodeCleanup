using System.ComponentModel;

using JetBrains.ReSharper.Feature.Services.CodeCleanup;

namespace XamlAttributeOrdering {
    [DefaultValue(true)]
    [DisplayName("Reorder XAML Attributes")]
    [Category(XamlCategory)]
    internal class Descriptor : CodeCleanupBoolOptionDescriptor {
        public Descriptor()
                : base("ReorderXamlAttributes") {
        }
    }
}