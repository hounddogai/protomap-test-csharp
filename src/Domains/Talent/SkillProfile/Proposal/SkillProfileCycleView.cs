namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile cycle.</summary>
public sealed record SkillProfileCycleView(string Code, string NameText, string RateText, string GraceDaysText, string Budget, string DurationMinutes, string Source, string CapacityText, string PhoneText, string Retries, string Rating, string StatusLabel)
{
    public static SkillProfileCycleView ToSkillProfileCycleView(SkillProfileCycle item)
    {
        return new SkillProfileCycleView(
            item.Code,
            item.Name,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 40),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileCycleView> ToSkillProfileCycleViews(List<SkillProfileCycle> items)
    {
        var views = new List<SkillProfileCycleView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileCycleView(item));
        }
        return views;
    }

    public static double SumSkillProfileCycleView(SkillProfileCycleView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedGraceDays + parsedBudget;
    }
}
