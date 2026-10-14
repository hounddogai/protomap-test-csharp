namespace Acme.Workforce.Compliance.PolicyAck.Assignment;

using System.Globalization;

/// <summary>How screens and exports show a policy ack score.</summary>
public sealed record PolicyAckScoreView(string Code, string LabelText, string ChecksumText, string GraceDaysText, string CountryText, string Fte, string RateText, string Minimum, string CostText, string UpdatedAt, string StatusLabel)
{
    public static PolicyAckScoreView ToPolicyAckScoreView(PolicyAckScore item)
    {
        return new PolicyAckScoreView(
            item.Code,
            item.Label,
            item.Checksum,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Country,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckScoreView> ToPolicyAckScoreViews(List<PolicyAckScore> items)
    {
        var views = new List<PolicyAckScoreView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckScoreView(item));
        }
        return views;
    }

    public static double SumPolicyAckScoreView(PolicyAckScoreView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedFte + parsedRate;
    }
}
