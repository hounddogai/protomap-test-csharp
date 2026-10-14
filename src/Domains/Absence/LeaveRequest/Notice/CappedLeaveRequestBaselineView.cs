namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped leave request baseline.</summary>
public sealed record CappedLeaveRequestBaselineView(string Code, string Title, string FteText, string BatchSize, string Capacity, string Cost, string Retries, string DescriptionText, string Country, string StatusLabel)
{
    public static CappedLeaveRequestBaselineView ToCappedLeaveRequestBaselineView(CappedLeaveRequestBaseline item)
    {
        return new CappedLeaveRequestBaselineView(
            item.Code,
            item.Title,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 120),
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedLeaveRequestBaselineView> ToCappedLeaveRequestBaselineViews(List<CappedLeaveRequestBaseline> items)
    {
        var views = new List<CappedLeaveRequestBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToCappedLeaveRequestBaselineView(item));
        }
        return views;
    }

    public static double SumCappedLeaveRequestBaselineView(CappedLeaveRequestBaselineView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedBatchSize + (double)parsedCapacity;
    }
}
