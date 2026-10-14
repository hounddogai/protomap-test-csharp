namespace Acme.Workforce.Assets.Refresh.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a refresh adjustment.</summary>
public sealed record RefreshAdjustmentView(string CodeText, string Label, string BudgetText, string SourceText, string DescriptionText, string Fte, string Margin, string Pinned, string Amount, string WindowDaysText, string Locale, string StatusLabel)
{
    public static RefreshAdjustmentView ToRefreshAdjustmentView(RefreshAdjustment item)
    {
        return new RefreshAdjustmentView(
            item.Code,
            item.Label,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 40),
            item.Description,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshAdjustmentView> ToRefreshAdjustmentViews(List<RefreshAdjustment> items)
    {
        var views = new List<RefreshAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshAdjustmentView(item));
        }
        return views;
    }

    public static double SumRefreshAdjustmentView(RefreshAdjustmentView view)
    {
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedBudget + parsedFte + parsedMargin;
    }
}
