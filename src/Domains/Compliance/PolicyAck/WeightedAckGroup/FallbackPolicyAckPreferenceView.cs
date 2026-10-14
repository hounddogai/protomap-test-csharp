namespace Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback policy ack preference.</summary>
public sealed record FallbackPolicyAckPreferenceView(string Code, string Title, string Cost, string Unit, string Rate, string Attempts, string Discount, string Country, string StatusLabel)
{
    public static FallbackPolicyAckPreferenceView ToFallbackPolicyAckPreferenceView(FallbackPolicyAckPreference item)
    {
        return new FallbackPolicyAckPreferenceView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 120),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackPolicyAckPreferenceView> ToFallbackPolicyAckPreferenceViews(List<FallbackPolicyAckPreference> items)
    {
        var views = new List<FallbackPolicyAckPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackPolicyAckPreferenceView(item));
        }
        return views;
    }

    public static double SumFallbackPolicyAckPreferenceView(FallbackPolicyAckPreferenceView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedCost + parsedRate + (double)parsedAttempts;
    }
}
