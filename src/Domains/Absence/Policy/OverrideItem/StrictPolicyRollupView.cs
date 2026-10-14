namespace Acme.Workforce.Absence.Policy.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict policy rollup.</summary>
public sealed record StrictPolicyRollupView(string Code, string LabelText, string Attempts, string CapacityText, string ContactEmail, string RatingText, string Rate, string Pinned, string ActiveText, string ExpiresAt, string StatusLabel)
{
    public static StrictPolicyRollupView ToStrictPolicyRollupView(StrictPolicyRollup item)
    {
        return new StrictPolicyRollupView(
            item.Code,
            item.Label,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 40),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            $"{item.Active}",
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictPolicyRollupView> ToStrictPolicyRollupViews(List<StrictPolicyRollup> items)
    {
        var views = new List<StrictPolicyRollupView>();
        foreach (var item in items)
        {
            views.Add(ToStrictPolicyRollupView(item));
        }
        return views;
    }

    public static double SumStrictPolicyRollupView(StrictPolicyRollupView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedCapacity + parsedRating;
    }
}
