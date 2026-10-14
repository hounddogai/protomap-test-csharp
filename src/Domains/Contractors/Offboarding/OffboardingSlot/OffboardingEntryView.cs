namespace Acme.Workforce.Contractors.Offboarding.OffboardingSlot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a offboarding entry.</summary>
public sealed record OffboardingEntryView(string Code, string Name, string SegmentText, string UnitText, string Title, string Score, string StatusLabel)
{
    public static OffboardingEntryView ToOffboardingEntryView(OffboardingEntry item)
    {
        return new OffboardingEntryView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Segment,
            item.Unit,
            item.Title,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingEntryView> ToOffboardingEntryViews(List<OffboardingEntry> items)
    {
        var views = new List<OffboardingEntryView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingEntryView(item));
        }
        return views;
    }

    public static double SumOffboardingEntryView(OffboardingEntryView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
