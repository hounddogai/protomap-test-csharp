namespace Acme.Workforce.Compliance.Exception.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a exception baseline.</summary>
public sealed record ExceptionBaselineView(string Code, string Label, string TenureMonths, string ExternalRef, string Retries, string StatusLabel)
{
    public static ExceptionBaselineView ToExceptionBaselineView(ExceptionBaseline item)
    {
        return new ExceptionBaselineView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 64),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 40),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionBaselineView> ToExceptionBaselineViews(List<ExceptionBaseline> items)
    {
        var views = new List<ExceptionBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionBaselineView(item));
        }
        return views;
    }

    public static double SumExceptionBaselineView(ExceptionBaselineView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedRetries;
    }
}
