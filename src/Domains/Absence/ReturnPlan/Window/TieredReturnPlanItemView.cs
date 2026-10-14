namespace Acme.Workforce.Absence.ReturnPlan.Window;

using System.Globalization;

/// <summary>How screens and exports show a tiered return plan item.</summary>
public sealed record TieredReturnPlanItemView(string Code, string Title, string Region, string OwnerId, string Quantity, string ExternalRef, string Location, string StatusLabel)
{
    public static TieredReturnPlanItemView ToTieredReturnPlanItemView(TieredReturnPlanItem item)
    {
        return new TieredReturnPlanItemView(
            item.Code,
            item.Title,
            item.Region,
            item.OwnerId,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredReturnPlanItemView> ToTieredReturnPlanItemViews(List<TieredReturnPlanItem> items)
    {
        var views = new List<TieredReturnPlanItemView>();
        foreach (var item in items)
        {
            views.Add(ToTieredReturnPlanItemView(item));
        }
        return views;
    }

    public static double SumTieredReturnPlanItemView(TieredReturnPlanItemView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
