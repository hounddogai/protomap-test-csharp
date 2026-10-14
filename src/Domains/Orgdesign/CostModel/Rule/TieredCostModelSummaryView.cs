namespace Acme.Workforce.Orgdesign.CostModel.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered cost model summary.</summary>
public sealed record TieredCostModelSummaryView(string Code, string TitleText, string Department, string Pinned, string UpdatedAt, string ManagerId, string Currency, string Rate, string EmployeeIdText, string StatusLabel)
{
    public static TieredCostModelSummaryView ToTieredCostModelSummaryView(TieredCostModelSummary item)
    {
        return new TieredCostModelSummaryView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Department,
            $"{item.Pinned}",
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 120),
            item.Currency,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredCostModelSummaryView> ToTieredCostModelSummaryViews(List<TieredCostModelSummary> items)
    {
        var views = new List<TieredCostModelSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToTieredCostModelSummaryView(item));
        }
        return views;
    }

    public static double SumTieredCostModelSummaryView(TieredCostModelSummaryView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedRate;
    }
}
