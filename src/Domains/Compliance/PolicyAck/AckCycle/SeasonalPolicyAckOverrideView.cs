namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal policy ack override.</summary>
public sealed record SeasonalPolicyAckOverrideView(string Code, string Name, string Capacity, string Rating, string Amount, string StatusLabel)
{
    public static SeasonalPolicyAckOverrideView ToSeasonalPolicyAckOverrideView(SeasonalPolicyAckOverride item)
    {
        return new SeasonalPolicyAckOverrideView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalPolicyAckOverrideView> ToSeasonalPolicyAckOverrideViews(List<SeasonalPolicyAckOverride> items)
    {
        var views = new List<SeasonalPolicyAckOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalPolicyAckOverrideView(item));
        }
        return views;
    }

    public static double SumSeasonalPolicyAckOverrideView(SeasonalPolicyAckOverrideView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedRating + parsedAmount;
    }
}
