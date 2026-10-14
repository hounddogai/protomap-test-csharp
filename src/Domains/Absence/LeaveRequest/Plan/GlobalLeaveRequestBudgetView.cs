namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global leave request budget.</summary>
public sealed record GlobalLeaveRequestBudgetView(string Code, string Name, string Grade, string ExternalRefText, string ScoreText, string Ceiling, string Ratio, string OwnerId, string Maximum, string StatusLabel)
{
    public static GlobalLeaveRequestBudgetView ToGlobalLeaveRequestBudgetView(GlobalLeaveRequestBudget item)
    {
        return new GlobalLeaveRequestBudgetView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Grade,
            Text.Truncate(item.ExternalRef, 80),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalLeaveRequestBudgetView> ToGlobalLeaveRequestBudgetViews(List<GlobalLeaveRequestBudget> items)
    {
        var views = new List<GlobalLeaveRequestBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalLeaveRequestBudgetView(item));
        }
        return views;
    }

    public static double SumGlobalLeaveRequestBudgetView(GlobalLeaveRequestBudgetView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedScore + parsedCeiling + parsedRatio;
    }
}
