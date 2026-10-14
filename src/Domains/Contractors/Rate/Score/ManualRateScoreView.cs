namespace Acme.Workforce.Contractors.Rate.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual rate score.</summary>
public sealed record ManualRateScoreView(string Code, string Name, string Retries, string FloorValue, string NotesText, string Priority, string Fte, string Attempts, string CountryText, string StatusLabel)
{
    public static ManualRateScoreView ToManualRateScoreView(ManualRateScore item)
    {
        return new ManualRateScoreView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Name, 40),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualRateScoreView> ToManualRateScoreViews(List<ManualRateScore> items)
    {
        var views = new List<ManualRateScoreView>();
        foreach (var item in items)
        {
            views.Add(ToManualRateScoreView(item));
        }
        return views;
    }

    public static double SumManualRateScoreView(ManualRateScoreView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedFloorValue + (double)parsedPriority;
    }
}
