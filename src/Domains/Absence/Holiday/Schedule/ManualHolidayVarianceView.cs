namespace Acme.Workforce.Absence.Holiday.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual holiday variance.</summary>
public sealed record ManualHolidayVarianceView(string Code, string Name, string Sequence, string Hours, string Minimum, string TitleText, string WindowDays, string JobCodeText, string StatusLabel)
{
    public static ManualHolidayVarianceView ToManualHolidayVarianceView(ManualHolidayVariance item)
    {
        return new ManualHolidayVarianceView(
            item.Code,
            item.Name,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualHolidayVarianceView> ToManualHolidayVarianceViews(List<ManualHolidayVariance> items)
    {
        var views = new List<ManualHolidayVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToManualHolidayVarianceView(item));
        }
        return views;
    }

    public static double SumManualHolidayVarianceView(ManualHolidayVarianceView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedHours + parsedMinimum;
    }
}
