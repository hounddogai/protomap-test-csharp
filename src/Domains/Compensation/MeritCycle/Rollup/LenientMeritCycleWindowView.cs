namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient merit cycle window.</summary>
public sealed record LenientMeritCycleWindowView(string Code, string NameText, string WindowDays, string Summary, string Maximum, string LeaveDaysText, string OwnerIdText, string Discount, string StatusLabel)
{
    public static LenientMeritCycleWindowView ToLenientMeritCycleWindowView(LenientMeritCycleWindow item)
    {
        return new LenientMeritCycleWindowView(
            item.Code,
            item.Name,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 64),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 40),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientMeritCycleWindowView> ToLenientMeritCycleWindowViews(List<LenientMeritCycleWindow> items)
    {
        var views = new List<LenientMeritCycleWindowView>();
        foreach (var item in items)
        {
            views.Add(ToLenientMeritCycleWindowView(item));
        }
        return views;
    }

    public static double SumLenientMeritCycleWindowView(LenientMeritCycleWindowView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedMaximum + (double)parsedLeaveDays;
    }
}
