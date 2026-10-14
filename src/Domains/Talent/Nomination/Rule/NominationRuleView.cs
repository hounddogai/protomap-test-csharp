namespace Acme.Workforce.Talent.Nomination.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a nomination rule.</summary>
public sealed record NominationRuleView(string CodeText, string Label, string Checksum, string SummaryText, string Title, string Channel, string Score, string StatusLabel)
{
    public static NominationRuleView ToNominationRuleView(NominationRule item)
    {
        return new NominationRuleView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Checksum,
            Text.Truncate(item.Summary, 40),
            item.Title,
            Text.Truncate(item.Channel, 40),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationRuleView> ToNominationRuleViews(List<NominationRule> items)
    {
        var views = new List<NominationRuleView>();
        foreach (var item in items)
        {
            views.Add(ToNominationRuleView(item));
        }
        return views;
    }

    public static double SumNominationRuleView(NominationRuleView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
