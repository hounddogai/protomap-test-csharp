namespace Acme.Workforce.Talent.Mobility.Profile;

using System.Globalization;

/// <summary>How screens and exports show a mobility profile.</summary>
public sealed record MobilityProfileView(string Code, string Title, string PriorityText, string EndsAt, string Fte, string StatusLabel)
{
    public static MobilityProfileView ToMobilityProfileView(MobilityProfile item)
    {
        return new MobilityProfileView(
            item.Code,
            item.Title,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityProfileView> ToMobilityProfileViews(List<MobilityProfile> items)
    {
        var views = new List<MobilityProfileView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityProfileView(item));
        }
        return views;
    }

    public static double SumMobilityProfileView(MobilityProfileView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedEndsAt + parsedFte;
    }
}
