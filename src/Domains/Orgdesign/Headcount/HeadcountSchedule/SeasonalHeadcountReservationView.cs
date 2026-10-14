namespace Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

using System.Globalization;

/// <summary>How screens and exports show a seasonal headcount reservation.</summary>
public sealed record SeasonalHeadcountReservationView(string CodeText, string TitleText, string Retries, string Department, string Cost, string Skill, string Phone, string StatusLabel)
{
    public static SeasonalHeadcountReservationView ToSeasonalHeadcountReservationView(SeasonalHeadcountReservation item)
    {
        return new SeasonalHeadcountReservationView(
            item.Code,
            item.Title,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalHeadcountReservationView> ToSeasonalHeadcountReservationViews(List<SeasonalHeadcountReservation> items)
    {
        var views = new List<SeasonalHeadcountReservationView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalHeadcountReservationView(item));
        }
        return views;
    }

    public static double SumSeasonalHeadcountReservationView(SeasonalHeadcountReservationView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedCost;
    }
}
