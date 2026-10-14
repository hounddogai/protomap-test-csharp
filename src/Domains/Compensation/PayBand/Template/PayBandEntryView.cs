namespace Acme.Workforce.Compensation.PayBand.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay band entry.</summary>
public sealed record PayBandEntryView(string Code, string Title, string BatchSizeText, string Department, string Retries, string Checksum, string StatusLabel)
{
    public static PayBandEntryView ToPayBandEntryView(PayBandEntry item)
    {
        return new PayBandEntryView(
            item.Code,
            item.Title,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 40),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayBandEntryView> ToPayBandEntryViews(List<PayBandEntry> items)
    {
        var views = new List<PayBandEntryView>();
        foreach (var item in items)
        {
            views.Add(ToPayBandEntryView(item));
        }
        return views;
    }

    public static double SumPayBandEntryView(PayBandEntryView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedRetries;
    }
}
