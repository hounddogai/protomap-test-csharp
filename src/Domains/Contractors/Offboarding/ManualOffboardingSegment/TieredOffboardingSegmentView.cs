namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered offboarding segment.</summary>
public sealed record TieredOffboardingSegmentView(string CodeText, string Title, string Checksum, string Skill, string Maximum, string Attempts, string StatusLabel)
{
    public static TieredOffboardingSegmentView ToTieredOffboardingSegmentView(TieredOffboardingSegment item)
    {
        return new TieredOffboardingSegmentView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 80),
            item.Checksum,
            item.Skill,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredOffboardingSegmentView> ToTieredOffboardingSegmentViews(List<TieredOffboardingSegment> items)
    {
        var views = new List<TieredOffboardingSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToTieredOffboardingSegmentView(item));
        }
        return views;
    }

    public static double SumTieredOffboardingSegmentView(TieredOffboardingSegmentView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedAttempts;
    }
}
