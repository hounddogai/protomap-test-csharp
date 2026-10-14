namespace Acme.Workforce.Workplace.Desk.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a desk rule.</summary>
public sealed record DeskRuleView(string CodeText, string Name, string Grade, string EndsAt, string Skill, string Source, string GraceDaysText, string Weight, string Unit, string DescriptionText, string StatusLabel)
{
    public static DeskRuleView ToDeskRuleView(DeskRule item)
    {
        return new DeskRuleView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Grade,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.Source,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            Text.Truncate(item.Description, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskRuleView> ToDeskRuleViews(List<DeskRule> items)
    {
        var views = new List<DeskRuleView>();
        foreach (var item in items)
        {
            views.Add(ToDeskRuleView(item));
        }
        return views;
    }

    public static double SumDeskRuleView(DeskRuleView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedGraceDays + parsedWeight;
    }
}
