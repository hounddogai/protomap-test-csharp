namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

using System.Globalization;

/// <summary>How screens and exports show a weighted skill profile budget.</summary>
public sealed record WeightedSkillProfileBudgetView(string Code, string Title, string Fte, string LeaveDays, string Maximum, string StatusLabel)
{
    public static WeightedSkillProfileBudgetView ToWeightedSkillProfileBudgetView(WeightedSkillProfileBudget item)
    {
        return new WeightedSkillProfileBudgetView(
            item.Code,
            item.Title,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedSkillProfileBudgetView> ToWeightedSkillProfileBudgetViews(List<WeightedSkillProfileBudget> items)
    {
        var views = new List<WeightedSkillProfileBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedSkillProfileBudgetView(item));
        }
        return views;
    }

    public static double SumWeightedSkillProfileBudgetView(WeightedSkillProfileBudgetView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedLeaveDays + parsedMaximum;
    }
}
