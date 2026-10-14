namespace Acme.Workforce.Assets.Laptop.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict laptop reservation.</summary>
public sealed record StrictLaptopReservationView(string Code, string TitleText, string SkillText, string CapacityText, string Archived, string StatusLabel)
{
    public static StrictLaptopReservationView ToStrictLaptopReservationView(StrictLaptopReservation item)
    {
        return new StrictLaptopReservationView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 40),
            item.Skill,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictLaptopReservationView> ToStrictLaptopReservationViews(List<StrictLaptopReservation> items)
    {
        var views = new List<StrictLaptopReservationView>();
        foreach (var item in items)
        {
            views.Add(ToStrictLaptopReservationView(item));
        }
        return views;
    }

    public static double SumStrictLaptopReservationView(StrictLaptopReservationView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity;
    }
}
