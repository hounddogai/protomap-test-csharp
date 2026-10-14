namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback pay run cycle.</summary>
public sealed record FallbackPayRunCycleView(string CodeText, string Title, string Hours, string AttemptsText, string ManagerId, string CreatedAt, string Margin, string Currency, string Limit, string Approved, string Source, string StatusLabel)
{
    public static FallbackPayRunCycleView ToFallbackPayRunCycleView(FallbackPayRunCycle item)
    {
        return new FallbackPayRunCycleView(
            item.Code,
            item.Title,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 64),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            Text.Truncate(item.Source, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackPayRunCycleView> ToFallbackPayRunCycleViews(List<FallbackPayRunCycle> items)
    {
        var views = new List<FallbackPayRunCycleView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackPayRunCycleView(item));
        }
        return views;
    }

    public static double SumFallbackPayRunCycleView(FallbackPayRunCycleView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedAttempts + (double)parsedCreatedAt;
    }
}
