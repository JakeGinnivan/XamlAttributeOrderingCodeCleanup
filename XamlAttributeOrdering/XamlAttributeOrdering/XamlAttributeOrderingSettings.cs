using JetBrains.Application.Settings;
using JetBrains.ReSharper.Settings;

namespace XamlAttributeOrdering
{
    [SettingsKey(typeof (CodeInspectionSettings), description: "Xaml Attribute Ordering Settings")]
    public class XamlAttributeOrderingSettings
    {
        [SettingsEntry(true, "Enable Ordering of Xaml Attributes")]
        public bool Enable { get; set; }

        // ReSharper disable InconsistentNaming
        [SettingsEntry("Key, x:Key", "Group #1 - Keys")]
        public string Group1_KeyGroup { get; set; }

        [SettingsEntry("Name, x:Name, Title", "Group #2 - Names")]
        public string Group2_NameGroup { get; set; }

        [SettingsEntry(
            "Grid.Row, Grid.RowSpan, Grid.Column, Grid.ColumnSpan, Canvas.Left, Canvas.Top, Canvas.Right, Canvas.Bottom", 
            "Group #3 - Attached Layout Properties")]
        public string Group3_AttachedLayoutGroup { get; set; }

        [SettingsEntry(
            "Width, Height, MinWidth, MinHeight, MaxWidth, MaxHeight, Margin",
            "Group #4 - Layout Properties")]
        public string Group4_LayoutGroup { get; set; }

        [SettingsEntry(
            "HorizontalAlignment, VerticalAlignment, HorizontalContentAlignment, VerticalContentAlignment, Panel.ZIndex", 
            "Group #5 - Alignment Properties")]
        public string Group5_AlignmentGroup { get; set; }

        [SettingsEntry(
            "PageSource, PageIndex, Offset, Color, TargetName, Property, Value, StartPoint, EndPoint",
            "Group #6 - Misc Properties")]
        public string Group6_MiscGroup { get; set; }
    }
}