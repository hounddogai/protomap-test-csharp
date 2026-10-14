namespace Acme.Workforce.Orgdesign.Headcount.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal headcount window.</summary>
public sealed record SeasonalHeadcountWindowView(string Code, string Label, string WindowDays, string Retries, string JobCode, string SkillText, string Description, string Sequence, string RegionText, string Margin, string Badge, string StatusLabel)
{
    public static SeasonalHeadcountWindowView ToSeasonalHeadcountWindowView(SeasonalHeadcountWindow item)
    {
        return new SeasonalHeadcountWindowView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            Text.Truncate(item.Skill, 40),
            Text.Truncate(item.Description, 120),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Region,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalHeadcountWindowView> ToSeasonalHeadcountWindowViews(List<SeasonalHeadcountWindow> items)
    {
        var views = new List<SeasonalHeadcountWindowView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalHeadcountWindowView(item));
        }
        return views;
    }

    public static double SumSeasonalHeadcountWindowView(SeasonalHeadcountWindowView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedRetries + (double)parsedSequence;
    }
}
