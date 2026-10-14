namespace Acme.Workforce.Assets.Refresh.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a refresh score.</summary>
public sealed record RefreshScoreView(string Code, string Name, string Budget, string Category, string Department, string EmployeeId, string Amount, string Ratio, string CostText, string RetriesText, string StatusLabel)
{
    public static RefreshScoreView ToRefreshScoreView(RefreshScore item)
    {
        return new RefreshScoreView(
            item.Code,
            item.Name,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 40),
            Text.Truncate(item.Department, 64),
            item.EmployeeId,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshScoreView> ToRefreshScoreViews(List<RefreshScore> items)
    {
        var views = new List<RefreshScoreView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshScoreView(item));
        }
        return views;
    }

    public static double SumRefreshScoreView(RefreshScoreView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedBudget + parsedAmount + parsedRatio;
    }
}
