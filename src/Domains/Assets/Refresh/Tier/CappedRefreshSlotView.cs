namespace Acme.Workforce.Assets.Refresh.Tier;

using System.Globalization;

/// <summary>How screens and exports show a capped refresh slot.</summary>
public sealed record CappedRefreshSlotView(string Code, string LabelText, string EmployeeIdText, string Retries, string Visible, string Capacity, string OwnerIdText, string StatusLabel)
{
    public static CappedRefreshSlotView ToCappedRefreshSlotView(CappedRefreshSlot item)
    {
        return new CappedRefreshSlotView(
            item.Code,
            item.Label,
            item.EmployeeId,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedRefreshSlotView> ToCappedRefreshSlotViews(List<CappedRefreshSlot> items)
    {
        var views = new List<CappedRefreshSlotView>();
        foreach (var item in items)
        {
            views.Add(ToCappedRefreshSlotView(item));
        }
        return views;
    }

    public static double SumCappedRefreshSlotView(CappedRefreshSlotView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedCapacity;
    }
}
