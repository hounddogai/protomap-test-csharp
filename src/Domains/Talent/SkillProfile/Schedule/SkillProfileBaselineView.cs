namespace Acme.Workforce.Talent.SkillProfile.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile baseline.</summary>
public sealed record SkillProfileBaselineView(string Code, string Title, string Amount, string Unit, string Badge, string Department, string Weight, string RateText, string EndsAtText, string Discount, string StatusLabel)
{
    public static SkillProfileBaselineView ToSkillProfileBaselineView(SkillProfileBaseline item)
    {
        return new SkillProfileBaselineView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 64),
            item.Badge,
            item.Department,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileBaselineView> ToSkillProfileBaselineViews(List<SkillProfileBaseline> items)
    {
        var views = new List<SkillProfileBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileBaselineView(item));
        }
        return views;
    }

    public static double SumSkillProfileBaselineView(SkillProfileBaselineView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return parsedAmount + parsedWeight + parsedRate;
    }
}
