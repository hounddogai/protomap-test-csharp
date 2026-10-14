namespace Acme.Workforce.Assets.Assignment.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a assignment preference.</summary>
public sealed record AssignmentPreferenceView(string CodeText, string Title, string Quantity, string SalaryBandText, string VisibleText, string Grade, string Category, string LocaleText, string StatusLabel)
{
    public static AssignmentPreferenceView ToAssignmentPreferenceView(AssignmentPreference item)
    {
        return new AssignmentPreferenceView(
            item.Code,
            item.Title,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            $"{item.Visible}",
            item.Grade,
            item.Category,
            Text.Truncate(item.Locale, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssignmentPreferenceView> ToAssignmentPreferenceViews(List<AssignmentPreference> items)
    {
        var views = new List<AssignmentPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToAssignmentPreferenceView(item));
        }
        return views;
    }

    public static double SumAssignmentPreferenceView(AssignmentPreferenceView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
