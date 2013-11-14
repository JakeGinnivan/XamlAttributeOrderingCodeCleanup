using System.Windows.Forms;
using JetBrains.ActionManagement;
using JetBrains.Application.DataContext;

namespace XamlAttributeOrdering
{
  [ActionHandler("XamlAttributeOrdering.About")]
  public class AboutAction : IActionHandler
  {
    public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
      // return true or false to enable/disable this action
      return true;
    }

    public void Execute(IDataContext context, DelegateExecute nextExecute)
    {
      MessageBox.Show(
        "XamlAttributeOrdering\nJake Ginnivan\n\nReorders Xaml Attributes",
        "About XamlAttributeOrdering",
        MessageBoxButtons.OK,
        MessageBoxIcon.Information);
    }
  }
}
