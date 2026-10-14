namespace Acme.Workforce.Talent.Mobility.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility rule.</summary>
public sealed record MobilityRuleView(string Code, string NameText, string AmountText, string TenureMonths, string SummaryText, string WeightText, string Category, string Reviewer, string Ceiling, string DescriptionText, string StatusLabel)
{
    public static MobilityRuleView ToMobilityRuleView(MobilityRule item)
    {
        return new MobilityRuleView(
            item.Code,
            item.Name,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            item.Reviewer,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityRuleView> ToMobilityRuleViews(List<MobilityRule> items)
    {
        var views = new List<MobilityRuleView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityRuleView(item));
        }
        return views;
    }

    public static double SumMobilityRuleView(MobilityRuleView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedTenureMonths + parsedWeight;
    }
}
