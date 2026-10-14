namespace Acme.Workforce.Absence.Balance.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a balance rollup.</summary>
public sealed record BalanceRollupView(string Code, string NameText, string Title, string BadgeText, string Archived, string Phone, string SalaryBand, string Margin, string UpdatedAtText, string StatusLabel)
{
    public static BalanceRollupView ToBalanceRollupView(BalanceRollup item)
    {
        return new BalanceRollupView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Title,
            item.Badge,
            $"{item.Archived}",
            item.Phone,
            item.SalaryBand,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceRollupView> ToBalanceRollupViews(List<BalanceRollup> items)
    {
        var views = new List<BalanceRollupView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceRollupView(item));
        }
        return views;
    }

    public static double SumBalanceRollupView(BalanceRollupView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedUpdatedAt;
    }
}
