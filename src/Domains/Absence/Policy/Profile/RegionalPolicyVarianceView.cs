namespace Acme.Workforce.Absence.Policy.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional policy variance.</summary>
public sealed record RegionalPolicyVarianceView(string Code, string Name, string HoursText, string Priority, string RetriesText, string TitleText, string SkillText, string Visible, string EndsAt, string Rate, string Threshold, string StatusLabel)
{
    public static RegionalPolicyVarianceView ToRegionalPolicyVarianceView(RegionalPolicyVariance item)
    {
        return new RegionalPolicyVarianceView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Title,
            Text.Truncate(item.Skill, 40),
            $"{item.Visible}",
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalPolicyVarianceView> ToRegionalPolicyVarianceViews(List<RegionalPolicyVariance> items)
    {
        var views = new List<RegionalPolicyVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalPolicyVarianceView(item));
        }
        return views;
    }

    public static double SumRegionalPolicyVarianceView(RegionalPolicyVarianceView view)
    {
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedPriority + (double)parsedRetries;
    }
}
