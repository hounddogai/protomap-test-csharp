namespace Acme.Workforce.Absence.Policy.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive policy bundle.</summary>
public sealed record AdaptivePolicyBundleView(string Code, string Label, string LocaleText, string Summary, string WindowDaysText, string Channel, string Segment, string FloorValue, string StatusLabel)
{
    public static AdaptivePolicyBundleView ToAdaptivePolicyBundleView(AdaptivePolicyBundle item)
    {
        return new AdaptivePolicyBundleView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Locale,
            Text.Truncate(item.Summary, 120),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Segment,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptivePolicyBundleView> ToAdaptivePolicyBundleViews(List<AdaptivePolicyBundle> items)
    {
        var views = new List<AdaptivePolicyBundleView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptivePolicyBundleView(item));
        }
        return views;
    }

    public static double SumAdaptivePolicyBundleView(AdaptivePolicyBundleView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedFloorValue;
    }
}
