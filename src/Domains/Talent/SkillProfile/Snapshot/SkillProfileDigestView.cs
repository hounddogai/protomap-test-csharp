namespace Acme.Workforce.Talent.SkillProfile.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a skill profile digest.</summary>
public sealed record SkillProfileDigestView(string CodeText, string Title, string ManagerId, string ExpiresAt, string GraceDaysText, string Channel, string BudgetText, string TimezoneText, string SalaryBand, string Visible, string StatusLabel)
{
    public static SkillProfileDigestView ToSkillProfileDigestView(SkillProfileDigest item)
    {
        return new SkillProfileDigestView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 120),
            item.ManagerId,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 64),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.SalaryBand,
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SkillProfileDigestView> ToSkillProfileDigestViews(List<SkillProfileDigest> items)
    {
        var views = new List<SkillProfileDigestView>();
        foreach (var item in items)
        {
            views.Add(ToSkillProfileDigestView(item));
        }
        return views;
    }

    public static double SumSkillProfileDigestView(SkillProfileDigestView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedGraceDays + parsedBudget;
    }
}
