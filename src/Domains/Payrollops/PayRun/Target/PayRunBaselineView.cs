namespace Acme.Workforce.Payrollops.PayRun.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay run baseline.</summary>
public sealed record PayRunBaselineView(string Code, string Title, string ArchivedText, string JobCodeText, string GraceDays, string ExpiresAt, string StatusLabel)
{
    public static PayRunBaselineView ToPayRunBaselineView(PayRunBaseline item)
    {
        return new PayRunBaselineView(
            Text.Truncate(item.Code, 80),
            item.Title,
            $"{item.Archived}",
            item.JobCode,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunBaselineView> ToPayRunBaselineViews(List<PayRunBaseline> items)
    {
        var views = new List<PayRunBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunBaselineView(item));
        }
        return views;
    }

    public static double SumPayRunBaselineView(PayRunBaselineView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedExpiresAt;
    }
}
