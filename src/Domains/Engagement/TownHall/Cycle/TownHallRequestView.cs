namespace Acme.Workforce.Engagement.TownHall.Cycle;

using System.Globalization;

/// <summary>How screens and exports show a town hall request.</summary>
public sealed record TownHallRequestView(string CodeText, string Label, string CreatedAt, string Phone, string ExpiresAt, string Notes, string GradeText, string LocationText, string Capacity, string FloorValue, string Archived, string StatusLabel)
{
    public static TownHallRequestView ToTownHallRequestView(TownHallRequest item)
    {
        return new TownHallRequestView(
            item.Code,
            item.Label,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.Grade,
            item.Location,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallRequestView> ToTownHallRequestViews(List<TownHallRequest> items)
    {
        var views = new List<TownHallRequestView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallRequestView(item));
        }
        return views;
    }

    public static double SumTownHallRequestView(TownHallRequestView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedExpiresAt + (double)parsedCapacity;
    }
}
