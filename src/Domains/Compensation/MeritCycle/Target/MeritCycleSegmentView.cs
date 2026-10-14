namespace Acme.Workforce.Compensation.MeritCycle.Target;

using System.Globalization;

/// <summary>How screens and exports show a merit cycle segment.</summary>
public sealed record MeritCycleSegmentView(string CodeText, string Title, string Description, string ExpiresAt, string Location, string GraceDays, string Rating, string StatusLabel)
{
    public static MeritCycleSegmentView ToMeritCycleSegmentView(MeritCycleSegment item)
    {
        return new MeritCycleSegmentView(
            item.Code,
            item.Title,
            item.Description,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleSegmentView> ToMeritCycleSegmentViews(List<MeritCycleSegment> items)
    {
        var views = new List<MeritCycleSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleSegmentView(item));
        }
        return views;
    }

    public static double SumMeritCycleSegmentView(MeritCycleSegmentView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedGraceDays + parsedRating;
    }
}
