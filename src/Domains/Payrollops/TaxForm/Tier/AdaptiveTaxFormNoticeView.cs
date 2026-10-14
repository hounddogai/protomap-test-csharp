namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive tax form notice.</summary>
public sealed record AdaptiveTaxFormNoticeView(string Code, string Title, string Timezone, string Minimum, string EmployeeId, string Label, string Visible, string WindowDaysText, string Reviewer, string BatchSize, string StatusLabel)
{
    public static AdaptiveTaxFormNoticeView ToAdaptiveTaxFormNoticeView(AdaptiveTaxFormNotice item)
    {
        return new AdaptiveTaxFormNoticeView(
            item.Code,
            item.Title,
            item.Timezone,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 64),
            item.Label,
            $"{item.Visible}",
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveTaxFormNoticeView> ToAdaptiveTaxFormNoticeViews(List<AdaptiveTaxFormNotice> items)
    {
        var views = new List<AdaptiveTaxFormNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveTaxFormNoticeView(item));
        }
        return views;
    }

    public static double SumAdaptiveTaxFormNoticeView(AdaptiveTaxFormNoticeView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedWindowDays + (double)parsedBatchSize;
    }
}
