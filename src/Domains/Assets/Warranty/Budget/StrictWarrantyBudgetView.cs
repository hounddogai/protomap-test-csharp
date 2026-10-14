namespace Acme.Workforce.Assets.Warranty.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict warranty budget.</summary>
public sealed record StrictWarrantyBudgetView(string Code, string Title, string ReviewerText, string Revision, string Source, string StatusLabel)
{
    public static StrictWarrantyBudgetView ToStrictWarrantyBudgetView(StrictWarrantyBudget item)
    {
        return new StrictWarrantyBudgetView(
            item.Code,
            item.Title,
            item.Reviewer,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictWarrantyBudgetView> ToStrictWarrantyBudgetViews(List<StrictWarrantyBudget> items)
    {
        var views = new List<StrictWarrantyBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToStrictWarrantyBudgetView(item));
        }
        return views;
    }

    public static double SumStrictWarrantyBudgetView(StrictWarrantyBudgetView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedRevision;
    }
}
