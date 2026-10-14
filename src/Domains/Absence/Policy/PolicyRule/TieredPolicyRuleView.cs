namespace Acme.Workforce.Absence.Policy.PolicyRule;

using System.Globalization;

/// <summary>How screens and exports show a tiered policy rule.</summary>
public sealed record TieredPolicyRuleView(string Code, string Name, string LeaveDays, string Segment, string Visible, string BatchSizeText, string StatusLabel)
{
    public static TieredPolicyRuleView ToTieredPolicyRuleView(TieredPolicyRule item)
    {
        return new TieredPolicyRuleView(
            item.Code,
            item.Name,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            $"{item.Visible}",
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredPolicyRuleView> ToTieredPolicyRuleViews(List<TieredPolicyRule> items)
    {
        var views = new List<TieredPolicyRuleView>();
        foreach (var item in items)
        {
            views.Add(ToTieredPolicyRuleView(item));
        }
        return views;
    }

    public static double SumTieredPolicyRuleView(TieredPolicyRuleView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedBatchSize;
    }
}
