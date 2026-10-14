namespace Acme.Workforce.Compliance.Attestation.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending attestation rule.</summary>
public sealed record PendingAttestationRuleView(string Code, string TitleText, string LabelText, string Rate, string FloorValue, string Visible, string Budget, string StatusLabel)
{
    public static PendingAttestationRuleView ToPendingAttestationRuleView(PendingAttestationRule item)
    {
        return new PendingAttestationRuleView(
            item.Code,
            Text.Truncate(item.Title, 40),
            Text.Truncate(item.Label, 120),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingAttestationRuleView> ToPendingAttestationRuleViews(List<PendingAttestationRule> items)
    {
        var views = new List<PendingAttestationRuleView>();
        foreach (var item in items)
        {
            views.Add(ToPendingAttestationRuleView(item));
        }
        return views;
    }

    public static double SumPendingAttestationRuleView(PendingAttestationRuleView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedRate + parsedFloorValue + parsedBudget;
    }
}
