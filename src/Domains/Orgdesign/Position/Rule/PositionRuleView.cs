namespace Acme.Workforce.Orgdesign.Position.Rule;

using System.Globalization;

/// <summary>How screens and exports show a position rule.</summary>
public sealed record PositionRuleView(string Code, string Label, string ManagerId, string Skill, string ContactEmail, string Amount, string StatusLabel)
{
    public static PositionRuleView ToPositionRuleView(PositionRule item)
    {
        return new PositionRuleView(
            item.Code,
            item.Label,
            item.ManagerId,
            item.Skill,
            item.ContactEmail,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionRuleView> ToPositionRuleViews(List<PositionRule> items)
    {
        var views = new List<PositionRuleView>();
        foreach (var item in items)
        {
            views.Add(ToPositionRuleView(item));
        }
        return views;
    }

    public static double SumPositionRuleView(PositionRuleView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
