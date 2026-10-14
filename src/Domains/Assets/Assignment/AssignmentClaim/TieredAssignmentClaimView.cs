namespace Acme.Workforce.Assets.Assignment.AssignmentClaim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered assignment claim.</summary>
public sealed record TieredAssignmentClaimView(string CodeText, string Name, string HoursText, string EndsAtText, string Threshold, string StatusLabel)
{
    public static TieredAssignmentClaimView ToTieredAssignmentClaimView(TieredAssignmentClaim item)
    {
        return new TieredAssignmentClaimView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Name, 80),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredAssignmentClaimView> ToTieredAssignmentClaimViews(List<TieredAssignmentClaim> items)
    {
        var views = new List<TieredAssignmentClaimView>();
        foreach (var item in items)
        {
            views.Add(ToTieredAssignmentClaimView(item));
        }
        return views;
    }

    public static double SumTieredAssignmentClaimView(TieredAssignmentClaimView view)
    {
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedEndsAt + parsedThreshold;
    }
}
