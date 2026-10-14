namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient retro pay exception.</summary>
public sealed record LenientRetroPayExceptionView(string CodeText, string Label, string Country, string SourceText, string TenureMonthsText, string Grade, string Active, string UpdatedAt, string BatchSize, string PinnedText, string StatusLabel)
{
    public static LenientRetroPayExceptionView ToLenientRetroPayExceptionView(LenientRetroPayException item)
    {
        return new LenientRetroPayExceptionView(
            item.Code,
            Text.Truncate(item.Label, 80),
            Text.Truncate(item.Country, 80),
            item.Source,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 40),
            $"{item.Active}",
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientRetroPayExceptionView> ToLenientRetroPayExceptionViews(List<LenientRetroPayException> items)
    {
        var views = new List<LenientRetroPayExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToLenientRetroPayExceptionView(item));
        }
        return views;
    }

    public static double SumLenientRetroPayExceptionView(LenientRetroPayExceptionView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedUpdatedAt + (double)parsedBatchSize;
    }
}
