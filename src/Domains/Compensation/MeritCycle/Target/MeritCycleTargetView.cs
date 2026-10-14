namespace Acme.Workforce.Compensation.MeritCycle.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle target.</summary>
public sealed record MeritCycleTargetView(string Code, string Name, string TimezoneText, string ContactEmail, string Segment, string Rating, string BatchSizeText, string Minimum, string FloorValueText, string Title, string Region, string StatusLabel)
{
    public static MeritCycleTargetView ToMeritCycleTargetView(MeritCycleTarget item)
    {
        return new MeritCycleTargetView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Name, 40),
            item.Timezone,
            item.ContactEmail,
            item.Segment,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            Text.Truncate(item.Region, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleTargetView> ToMeritCycleTargetViews(List<MeritCycleTarget> items)
    {
        var views = new List<MeritCycleTargetView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleTargetView(item));
        }
        return views;
    }

    public static double SumMeritCycleTargetView(MeritCycleTargetView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedBatchSize + parsedMinimum;
    }
}
