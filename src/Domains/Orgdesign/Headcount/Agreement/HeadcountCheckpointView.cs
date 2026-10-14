namespace Acme.Workforce.Orgdesign.Headcount.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a headcount checkpoint.</summary>
public sealed record HeadcountCheckpointView(string Code, string NameText, string CategoryText, string ContactEmailText, string DurationMinutes, string Archived, string StatusLabel)
{
    public static HeadcountCheckpointView ToHeadcountCheckpointView(HeadcountCheckpoint item)
    {
        return new HeadcountCheckpointView(
            item.Code,
            item.Name,
            Text.Truncate(item.Category, 120),
            item.ContactEmail,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountCheckpointView> ToHeadcountCheckpointViews(List<HeadcountCheckpoint> items)
    {
        var views = new List<HeadcountCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountCheckpointView(item));
        }
        return views;
    }

    public static double SumHeadcountCheckpointView(HeadcountCheckpointView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes;
    }
}
