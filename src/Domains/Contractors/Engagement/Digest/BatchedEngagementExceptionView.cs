namespace Acme.Workforce.Contractors.Engagement.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched engagement exception.</summary>
public sealed record BatchedEngagementExceptionView(string CodeText, string LabelText, string Margin, string ChecksumText, string WindowDaysText, string TenureMonthsText, string Name, string Weight, string Department, string TeamText, string StatusLabel)
{
    public static BatchedEngagementExceptionView ToBatchedEngagementExceptionView(BatchedEngagementException item)
    {
        return new BatchedEngagementExceptionView(
            item.Code,
            item.Label,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 80),
            Text.Truncate(item.Team, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedEngagementExceptionView> ToBatchedEngagementExceptionViews(List<BatchedEngagementException> items)
    {
        var views = new List<BatchedEngagementExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedEngagementExceptionView(item));
        }
        return views;
    }

    public static double SumBatchedEngagementExceptionView(BatchedEngagementExceptionView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedWindowDays + (double)parsedTenureMonths;
    }
}
