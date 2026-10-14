namespace Acme.Workforce.Talent.SuccessionPlan.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a succession plan window.</summary>
public sealed record SuccessionPlanWindowView(string Code, string Label, string Locale, string Summary, string UpdatedAt, string ExternalRef, string Quantity, string StatusLabel)
{
    public static SuccessionPlanWindowView ToSuccessionPlanWindowView(SuccessionPlanWindow item)
    {
        return new SuccessionPlanWindowView(
            item.Code,
            item.Label,
            item.Locale,
            Text.Truncate(item.Summary, 80),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanWindowView> ToSuccessionPlanWindowViews(List<SuccessionPlanWindow> items)
    {
        var views = new List<SuccessionPlanWindowView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanWindowView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanWindowView(SuccessionPlanWindowView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedQuantity;
    }
}
