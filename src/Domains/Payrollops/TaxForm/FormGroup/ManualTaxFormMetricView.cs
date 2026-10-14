namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual tax form metric.</summary>
public sealed record ManualTaxFormMetricView(string CodeText, string TitleText, string ExpiresAt, string CategoryText, string Active, string Rating, string ChannelText, string JobCode, string StatusLabel)
{
    public static ManualTaxFormMetricView ToManualTaxFormMetricView(ManualTaxFormMetric item)
    {
        return new ManualTaxFormMetricView(
            item.Code,
            item.Title,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Category,
            $"{item.Active}",
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 40),
            Text.Truncate(item.JobCode, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualTaxFormMetricView> ToManualTaxFormMetricViews(List<ManualTaxFormMetric> items)
    {
        var views = new List<ManualTaxFormMetricView>();
        foreach (var item in items)
        {
            views.Add(ToManualTaxFormMetricView(item));
        }
        return views;
    }

    public static double SumManualTaxFormMetricView(ManualTaxFormMetricView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedRating;
    }
}
