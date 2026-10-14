namespace Acme.Workforce.Workplace.Occupancy.Ledger;

using System.Globalization;

/// <summary>How screens and exports show a occupancy cycle.</summary>
public sealed record OccupancyCycleView(string CodeText, string Name, string PinnedText, string Active, string SalaryBandText, string Amount, string StatusLabel)
{
    public static OccupancyCycleView ToOccupancyCycleView(OccupancyCycle item)
    {
        return new OccupancyCycleView(
            item.Code,
            item.Name,
            $"{item.Pinned}",
            $"{item.Active}",
            item.SalaryBand,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyCycleView> ToOccupancyCycleViews(List<OccupancyCycle> items)
    {
        var views = new List<OccupancyCycleView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyCycleView(item));
        }
        return views;
    }

    public static double SumOccupancyCycleView(OccupancyCycleView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
