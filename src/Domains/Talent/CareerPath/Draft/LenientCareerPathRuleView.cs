namespace Acme.Workforce.Talent.CareerPath.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient career path rule.</summary>
public sealed record LenientCareerPathRuleView(string Code, string Name, string SourceText, string Skill, string CreatedAt, string ManagerId, string Channel, string RateText, string ApprovedText, string StatusLabel)
{
    public static LenientCareerPathRuleView ToLenientCareerPathRuleView(LenientCareerPathRule item)
    {
        return new LenientCareerPathRuleView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Source,
            item.Skill,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Channel,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientCareerPathRuleView> ToLenientCareerPathRuleViews(List<LenientCareerPathRule> items)
    {
        var views = new List<LenientCareerPathRuleView>();
        foreach (var item in items)
        {
            views.Add(ToLenientCareerPathRuleView(item));
        }
        return views;
    }

    public static double SumLenientCareerPathRuleView(LenientCareerPathRuleView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedRate;
    }
}
