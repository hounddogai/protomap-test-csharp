namespace Acme.Workforce.Orgdesign.Span.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered span schedule.</summary>
public sealed record TieredSpanScheduleView(string Code, string NameText, string BudgetText, string JobCode, string NotesText, string Limit, string StatusLabel)
{
    public static TieredSpanScheduleView ToTieredSpanScheduleView(TieredSpanSchedule item)
    {
        return new TieredSpanScheduleView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            item.Notes,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredSpanScheduleView> ToTieredSpanScheduleViews(List<TieredSpanSchedule> items)
    {
        var views = new List<TieredSpanScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToTieredSpanScheduleView(item));
        }
        return views;
    }

    public static double SumTieredSpanScheduleView(TieredSpanScheduleView view)
    {
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return parsedBudget + (double)parsedLimit;
    }
}
