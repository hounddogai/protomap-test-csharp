namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using System.Globalization;

/// <summary>How screens and exports show a primary scenario segment.</summary>
public sealed record PrimaryScenarioSegmentView(string CodeText, string Title, string DurationMinutesText, string Revision, string PinnedText, string StatusLabel)
{
    public static PrimaryScenarioSegmentView ToPrimaryScenarioSegmentView(PrimaryScenarioSegment item)
    {
        return new PrimaryScenarioSegmentView(
            item.Code,
            item.Title,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryScenarioSegmentView> ToPrimaryScenarioSegmentViews(List<PrimaryScenarioSegment> items)
    {
        var views = new List<PrimaryScenarioSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryScenarioSegmentView(item));
        }
        return views;
    }

    public static double SumPrimaryScenarioSegmentView(PrimaryScenarioSegmentView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedRevision;
    }
}
