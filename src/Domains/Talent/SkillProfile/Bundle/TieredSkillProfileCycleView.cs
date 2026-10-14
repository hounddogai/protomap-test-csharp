namespace Acme.Workforce.Talent.SkillProfile.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered skill profile cycle.</summary>
public sealed record TieredSkillProfileCycleView(string Code, string Label, string Attempts, string Timezone, string LeaveDaysText, string SalaryBand, string StatusLabel)
{
    public static TieredSkillProfileCycleView ToTieredSkillProfileCycleView(TieredSkillProfileCycle item)
    {
        return new TieredSkillProfileCycleView(
            item.Code,
            item.Label,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 64),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredSkillProfileCycleView> ToTieredSkillProfileCycleViews(List<TieredSkillProfileCycle> items)
    {
        var views = new List<TieredSkillProfileCycleView>();
        foreach (var item in items)
        {
            views.Add(ToTieredSkillProfileCycleView(item));
        }
        return views;
    }

    public static double SumTieredSkillProfileCycleView(TieredSkillProfileCycleView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedLeaveDays;
    }
}
