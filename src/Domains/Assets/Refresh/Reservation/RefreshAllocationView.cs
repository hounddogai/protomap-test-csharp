namespace Acme.Workforce.Assets.Refresh.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a refresh allocation.</summary>
public sealed record RefreshAllocationView(string Code, string Name, string StartsAt, string Active, string Version, string StatusLabel)
{
    public static RefreshAllocationView ToRefreshAllocationView(RefreshAllocation item)
    {
        return new RefreshAllocationView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshAllocationView> ToRefreshAllocationViews(List<RefreshAllocation> items)
    {
        var views = new List<RefreshAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshAllocationView(item));
        }
        return views;
    }

    public static double SumRefreshAllocationView(RefreshAllocationView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedVersion;
    }
}
