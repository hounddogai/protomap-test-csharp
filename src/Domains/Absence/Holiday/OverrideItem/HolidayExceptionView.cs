namespace Acme.Workforce.Absence.Holiday.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a holiday exception.</summary>
public sealed record HolidayExceptionView(string Code, string Title, string Rating, string Fte, string OwnerId, string EmployeeIdText, string Unit, string CostCenter, string Country, string StatusLabel)
{
    public static HolidayExceptionView ToHolidayExceptionView(HolidayException item)
    {
        return new HolidayExceptionView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            item.EmployeeId,
            item.Unit,
            item.CostCenter,
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HolidayExceptionView> ToHolidayExceptionViews(List<HolidayException> items)
    {
        var views = new List<HolidayExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToHolidayExceptionView(item));
        }
        return views;
    }

    public static double SumHolidayExceptionView(HolidayExceptionView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedRating + parsedFte;
    }
}
