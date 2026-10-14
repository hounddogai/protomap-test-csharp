namespace Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

using System.Globalization;

/// <summary>How screens and exports show a adaptive offboarding segment.</summary>
public sealed record AdaptiveOffboardingSegmentView(string Code, string Title, string Margin, string Amount, string WindowDays, string StatusLabel)
{
    public static AdaptiveOffboardingSegmentView ToAdaptiveOffboardingSegmentView(AdaptiveOffboardingSegment item)
    {
        return new AdaptiveOffboardingSegmentView(
            item.Code,
            item.Title,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveOffboardingSegmentView> ToAdaptiveOffboardingSegmentViews(List<AdaptiveOffboardingSegment> items)
    {
        var views = new List<AdaptiveOffboardingSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveOffboardingSegmentView(item));
        }
        return views;
    }

    public static double SumAdaptiveOffboardingSegmentView(AdaptiveOffboardingSegmentView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return parsedMargin + parsedAmount + (double)parsedWindowDays;
    }
}
