namespace Acme.Workforce.Assets.Refresh.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a refresh estimate.</summary>
public sealed record RefreshEstimateView(string Code, string NameText, string ManagerId, string GraceDays, string Attempts, string Version, string RetriesText, string ApprovedText, string Description, string SummaryText, string BadgeText, string StatusLabel)
{
    public static RefreshEstimateView ToRefreshEstimateView(RefreshEstimate item)
    {
        return new RefreshEstimateView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.ManagerId,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Description,
            item.Summary,
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshEstimateView> ToRefreshEstimateViews(List<RefreshEstimate> items)
    {
        var views = new List<RefreshEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshEstimateView(item));
        }
        return views;
    }

    public static double SumRefreshEstimateView(RefreshEstimateView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedAttempts + (double)parsedVersion;
    }
}
