namespace Acme.Workforce.Absence.Accrual.Batch;

using System.Globalization;

/// <summary>How screens and exports show a accrual estimate.</summary>
public sealed record AccrualEstimateView(string Code, string NameText, string WindowDaysText, string FloorValue, string Score, string StatusLabel)
{
    public static AccrualEstimateView ToAccrualEstimateView(AccrualEstimate item)
    {
        return new AccrualEstimateView(
            item.Code,
            item.Name,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualEstimateView> ToAccrualEstimateViews(List<AccrualEstimate> items)
    {
        var views = new List<AccrualEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualEstimateView(item));
        }
        return views;
    }

    public static double SumAccrualEstimateView(AccrualEstimateView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedFloorValue + parsedScore;
    }
}
