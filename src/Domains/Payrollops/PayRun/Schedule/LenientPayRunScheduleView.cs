namespace Acme.Workforce.Payrollops.PayRun.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient pay run schedule.</summary>
public sealed record LenientPayRunScheduleView(string Code, string LabelText, string Active, string Amount, string TitleText, string Department, string Fte, string StatusLabel)
{
    public static LenientPayRunScheduleView ToLenientPayRunScheduleView(LenientPayRunSchedule item)
    {
        return new LenientPayRunScheduleView(
            item.Code,
            item.Label,
            $"{item.Active}",
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Title, 40),
            Text.Truncate(item.Department, 64),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientPayRunScheduleView> ToLenientPayRunScheduleViews(List<LenientPayRunSchedule> items)
    {
        var views = new List<LenientPayRunScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToLenientPayRunScheduleView(item));
        }
        return views;
    }

    public static double SumLenientPayRunScheduleView(LenientPayRunScheduleView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedAmount + parsedFte;
    }
}
