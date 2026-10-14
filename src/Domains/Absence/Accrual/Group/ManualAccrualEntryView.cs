namespace Acme.Workforce.Absence.Accrual.Group;

using System.Globalization;

/// <summary>How screens and exports show a manual accrual entry.</summary>
public sealed record ManualAccrualEntryView(string Code, string Label, string Budget, string SkillText, string Rating, string StatusLabel)
{
    public static ManualAccrualEntryView ToManualAccrualEntryView(ManualAccrualEntry item)
    {
        return new ManualAccrualEntryView(
            item.Code,
            item.Label,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualAccrualEntryView> ToManualAccrualEntryViews(List<ManualAccrualEntry> items)
    {
        var views = new List<ManualAccrualEntryView>();
        foreach (var item in items)
        {
            views.Add(ToManualAccrualEntryView(item));
        }
        return views;
    }

    public static double SumManualAccrualEntryView(ManualAccrualEntryView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedBudget + parsedRating;
    }
}
