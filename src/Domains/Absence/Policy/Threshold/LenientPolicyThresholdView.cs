namespace Acme.Workforce.Absence.Policy.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient policy threshold.</summary>
public sealed record LenientPolicyThresholdView(string Code, string Name, string DurationMinutesText, string Approved, string Active, string GraceDays, string StatusLabel)
{
    public static LenientPolicyThresholdView ToLenientPolicyThresholdView(LenientPolicyThreshold item)
    {
        return new LenientPolicyThresholdView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Active}",
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientPolicyThresholdView> ToLenientPolicyThresholdViews(List<LenientPolicyThreshold> items)
    {
        var views = new List<LenientPolicyThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToLenientPolicyThresholdView(item));
        }
        return views;
    }

    public static double SumLenientPolicyThresholdView(LenientPolicyThresholdView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedGraceDays;
    }
}
