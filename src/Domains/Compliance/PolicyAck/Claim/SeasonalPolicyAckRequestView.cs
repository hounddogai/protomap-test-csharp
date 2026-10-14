namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using System.Globalization;

/// <summary>How screens and exports show a seasonal policy ack request.</summary>
public sealed record SeasonalPolicyAckRequestView(string CodeText, string LabelText, string Approved, string Cost, string StatusLabel)
{
    public static SeasonalPolicyAckRequestView ToSeasonalPolicyAckRequestView(SeasonalPolicyAckRequest item)
    {
        return new SeasonalPolicyAckRequestView(
            item.Code,
            item.Label,
            $"{item.Approved}",
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalPolicyAckRequestView> ToSeasonalPolicyAckRequestViews(List<SeasonalPolicyAckRequest> items)
    {
        var views = new List<SeasonalPolicyAckRequestView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalPolicyAckRequestView(item));
        }
        return views;
    }

    public static double SumSeasonalPolicyAckRequestView(SeasonalPolicyAckRequestView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedCost;
    }
}
