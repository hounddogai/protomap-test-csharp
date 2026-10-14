namespace Acme.Workforce.Assets.Refresh.Request;

using System.Globalization;

/// <summary>How screens and exports show a pending refresh mapping.</summary>
public sealed record PendingRefreshMappingView(string Code, string LabelText, string Capacity, string Reviewer, string Limit, string StartsAt, string StatusLabel)
{
    public static PendingRefreshMappingView ToPendingRefreshMappingView(PendingRefreshMapping item)
    {
        return new PendingRefreshMappingView(
            item.Code,
            item.Label,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingRefreshMappingView> ToPendingRefreshMappingViews(List<PendingRefreshMapping> items)
    {
        var views = new List<PendingRefreshMappingView>();
        foreach (var item in items)
        {
            views.Add(ToPendingRefreshMappingView(item));
        }
        return views;
    }

    public static double SumPendingRefreshMappingView(PendingRefreshMappingView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedLimit + (double)parsedStartsAt;
    }
}
