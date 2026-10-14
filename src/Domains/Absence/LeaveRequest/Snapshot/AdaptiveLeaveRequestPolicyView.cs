namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive leave request policy.</summary>
public sealed record AdaptiveLeaveRequestPolicyView(string CodeText, string Name, string Ratio, string Retries, string MarginText, string MinimumText, string Category, string JobCode, string StatusLabel)
{
    public static AdaptiveLeaveRequestPolicyView ToAdaptiveLeaveRequestPolicyView(AdaptiveLeaveRequestPolicy item)
    {
        return new AdaptiveLeaveRequestPolicyView(
            item.Code,
            item.Name,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            Text.Truncate(item.JobCode, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveLeaveRequestPolicyView> ToAdaptiveLeaveRequestPolicyViews(List<AdaptiveLeaveRequestPolicy> items)
    {
        var views = new List<AdaptiveLeaveRequestPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveLeaveRequestPolicyView(item));
        }
        return views;
    }

    public static double SumAdaptiveLeaveRequestPolicyView(AdaptiveLeaveRequestPolicyView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedRetries + parsedMargin;
    }
}
