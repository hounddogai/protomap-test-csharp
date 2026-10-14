namespace Acme.Workforce.Contractors.Engagement.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback engagement exception.</summary>
public sealed record FallbackEngagementExceptionView(string CodeText, string Label, string SegmentText, string TimezoneText, string ExternalRef, string LeaveDaysText, string Rate, string CostCenter, string StatusLabel)
{
    public static FallbackEngagementExceptionView ToFallbackEngagementExceptionView(FallbackEngagementException item)
    {
        return new FallbackEngagementExceptionView(
            item.Code,
            item.Label,
            Text.Truncate(item.Segment, 40),
            Text.Truncate(item.Timezone, 120),
            item.ExternalRef,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackEngagementExceptionView> ToFallbackEngagementExceptionViews(List<FallbackEngagementException> items)
    {
        var views = new List<FallbackEngagementExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackEngagementExceptionView(item));
        }
        return views;
    }

    public static double SumFallbackEngagementExceptionView(FallbackEngagementExceptionView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedRate;
    }
}
