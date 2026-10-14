namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted policy ack profile.</summary>
public sealed record WeightedPolicyAckProfileView(string CodeText, string TitleText, string WeightText, string Country, string Notes, string Fte, string StartsAtText, string StatusLabel)
{
    public static WeightedPolicyAckProfileView ToWeightedPolicyAckProfileView(WeightedPolicyAckProfile item)
    {
        return new WeightedPolicyAckProfileView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 80),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            item.Notes,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedPolicyAckProfileView> ToWeightedPolicyAckProfileViews(List<WeightedPolicyAckProfile> items)
    {
        var views = new List<WeightedPolicyAckProfileView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedPolicyAckProfileView(item));
        }
        return views;
    }

    public static double SumWeightedPolicyAckProfileView(WeightedPolicyAckProfileView view)
    {
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return parsedWeight + parsedFte + (double)parsedStartsAt;
    }
}
