namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling benchmark profile.</summary>
public sealed record RollingBenchmarkProfileView(string CodeText, string Title, string OwnerId, string ContactEmail, string LeaveDays, string Amount, string PriorityText, string CountryText, string StatusLabel)
{
    public static RollingBenchmarkProfileView ToRollingBenchmarkProfileView(RollingBenchmarkProfile item)
    {
        return new RollingBenchmarkProfileView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.OwnerId,
            item.ContactEmail,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingBenchmarkProfileView> ToRollingBenchmarkProfileViews(List<RollingBenchmarkProfile> items)
    {
        var views = new List<RollingBenchmarkProfileView>();
        foreach (var item in items)
        {
            views.Add(ToRollingBenchmarkProfileView(item));
        }
        return views;
    }

    public static double SumRollingBenchmarkProfileView(RollingBenchmarkProfileView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedAmount + (double)parsedPriority;
    }
}
