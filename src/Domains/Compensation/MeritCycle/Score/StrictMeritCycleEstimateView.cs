namespace Acme.Workforce.Compensation.MeritCycle.Score;

using System.Globalization;

/// <summary>How screens and exports show a strict merit cycle estimate.</summary>
public sealed record StrictMeritCycleEstimateView(string CodeText, string LabelText, string NotesText, string Location, string Amount, string StatusLabel)
{
    public static StrictMeritCycleEstimateView ToStrictMeritCycleEstimateView(StrictMeritCycleEstimate item)
    {
        return new StrictMeritCycleEstimateView(
            item.Code,
            item.Label,
            item.Notes,
            item.Location,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictMeritCycleEstimateView> ToStrictMeritCycleEstimateViews(List<StrictMeritCycleEstimate> items)
    {
        var views = new List<StrictMeritCycleEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToStrictMeritCycleEstimateView(item));
        }
        return views;
    }

    public static double SumStrictMeritCycleEstimateView(StrictMeritCycleEstimateView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
