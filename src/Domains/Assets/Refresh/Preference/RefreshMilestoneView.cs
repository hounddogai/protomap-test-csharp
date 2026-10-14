namespace Acme.Workforce.Assets.Refresh.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a refresh milestone.</summary>
public sealed record RefreshMilestoneView(string Code, string Label, string Category, string ExpiresAtText, string Checksum, string Amount, string StatusLabel)
{
    public static RefreshMilestoneView ToRefreshMilestoneView(RefreshMilestone item)
    {
        return new RefreshMilestoneView(
            item.Code,
            item.Label,
            Text.Truncate(item.Category, 64),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshMilestoneView> ToRefreshMilestoneViews(List<RefreshMilestone> items)
    {
        var views = new List<RefreshMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshMilestoneView(item));
        }
        return views;
    }

    public static double SumRefreshMilestoneView(RefreshMilestoneView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedAmount;
    }
}
