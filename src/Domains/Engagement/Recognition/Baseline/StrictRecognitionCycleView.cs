namespace Acme.Workforce.Engagement.Recognition.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict recognition cycle.</summary>
public sealed record StrictRecognitionCycleView(string Code, string Name, string DurationMinutesText, string Archived, string Ratio, string Limit, string Budget, string StatusLabel)
{
    public static StrictRecognitionCycleView ToStrictRecognitionCycleView(StrictRecognitionCycle item)
    {
        return new StrictRecognitionCycleView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictRecognitionCycleView> ToStrictRecognitionCycleViews(List<StrictRecognitionCycle> items)
    {
        var views = new List<StrictRecognitionCycleView>();
        foreach (var item in items)
        {
            views.Add(ToStrictRecognitionCycleView(item));
        }
        return views;
    }

    public static double SumStrictRecognitionCycleView(StrictRecognitionCycleView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedRatio + (double)parsedLimit;
    }
}
