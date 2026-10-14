namespace Acme.Workforce.Absence.Holiday.HolidayQuota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient holiday cycle.</summary>
public sealed record LenientHolidayCycleView(string Code, string Name, string Maximum, string Team, string Amount, string StatusLabel)
{
    public static LenientHolidayCycleView ToLenientHolidayCycleView(LenientHolidayCycle item)
    {
        return new LenientHolidayCycleView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientHolidayCycleView> ToLenientHolidayCycleViews(List<LenientHolidayCycle> items)
    {
        var views = new List<LenientHolidayCycleView>();
        foreach (var item in items)
        {
            views.Add(ToLenientHolidayCycleView(item));
        }
        return views;
    }

    public static double SumLenientHolidayCycleView(LenientHolidayCycleView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedAmount;
    }
}
