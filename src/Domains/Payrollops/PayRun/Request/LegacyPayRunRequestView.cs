namespace Acme.Workforce.Payrollops.PayRun.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy pay run request.</summary>
public sealed record LegacyPayRunRequestView(string Code, string TitleText, string Channel, string Budget, string SkillText, string Attempts, string TenureMonthsText, string UpdatedAt, string StatusLabel)
{
    public static LegacyPayRunRequestView ToLegacyPayRunRequestView(LegacyPayRunRequest item)
    {
        return new LegacyPayRunRequestView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.Channel,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyPayRunRequestView> ToLegacyPayRunRequestViews(List<LegacyPayRunRequest> items)
    {
        var views = new List<LegacyPayRunRequestView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyPayRunRequestView(item));
        }
        return views;
    }

    public static double SumLegacyPayRunRequestView(LegacyPayRunRequestView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return parsedBudget + (double)parsedAttempts + (double)parsedTenureMonths;
    }
}
