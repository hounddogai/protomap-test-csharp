namespace Acme.Workforce.Contractors.Rate.RateProposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted rate preference.</summary>
public sealed record WeightedRatePreferenceView(string Code, string TitleText, string LeaveDays, string ActiveText, string EndsAt, string JobCode, string Skill, string StatusLabel)
{
    public static WeightedRatePreferenceView ToWeightedRatePreferenceView(WeightedRatePreference item)
    {
        return new WeightedRatePreferenceView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            Text.Truncate(item.Skill, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedRatePreferenceView> ToWeightedRatePreferenceViews(List<WeightedRatePreference> items)
    {
        var views = new List<WeightedRatePreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedRatePreferenceView(item));
        }
        return views;
    }

    public static double SumWeightedRatePreferenceView(WeightedRatePreferenceView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedEndsAt;
    }
}
