namespace Acme.Workforce.Compensation.BonusPool.PoolSchedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal bonus pool schedule.</summary>
public sealed record SeasonalBonusPoolScheduleView(string CodeText, string TitleText, string Reviewer, string UpdatedAt, string Cost, string ManagerId, string Budget, string Priority, string Unit, string Active, string VisibleText, string StatusLabel)
{
    public static SeasonalBonusPoolScheduleView ToSeasonalBonusPoolScheduleView(SeasonalBonusPoolSchedule item)
    {
        return new SeasonalBonusPoolScheduleView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 40),
            item.Reviewer,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            $"{item.Active}",
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalBonusPoolScheduleView> ToSeasonalBonusPoolScheduleViews(List<SeasonalBonusPoolSchedule> items)
    {
        var views = new List<SeasonalBonusPoolScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalBonusPoolScheduleView(item));
        }
        return views;
    }

    public static double SumSeasonalBonusPoolScheduleView(SeasonalBonusPoolScheduleView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedCost + parsedBudget;
    }
}
