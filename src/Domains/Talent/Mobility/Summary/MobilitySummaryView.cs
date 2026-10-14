namespace Acme.Workforce.Talent.Mobility.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility summary.</summary>
public sealed record MobilitySummaryView(string Code, string Title, string ContactEmail, string TeamText, string BudgetText, string Pinned, string TenureMonths, string Department, string StatusLabel)
{
    public static MobilitySummaryView ToMobilitySummaryView(MobilitySummary item)
    {
        return new MobilitySummaryView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.ContactEmail,
            item.Team,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilitySummaryView> ToMobilitySummaryViews(List<MobilitySummary> items)
    {
        var views = new List<MobilitySummaryView>();
        foreach (var item in items)
        {
            views.Add(ToMobilitySummaryView(item));
        }
        return views;
    }

    public static double SumMobilitySummaryView(MobilitySummaryView view)
    {
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return parsedBudget + (double)parsedTenureMonths;
    }
}
