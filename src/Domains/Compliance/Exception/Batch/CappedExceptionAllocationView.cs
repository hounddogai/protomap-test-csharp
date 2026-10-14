namespace Acme.Workforce.Compliance.Exception.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped exception allocation.</summary>
public sealed record CappedExceptionAllocationView(string Code, string Label, string WeightText, string TenureMonths, string Fte, string Location, string DurationMinutes, string Cost, string StatusLabel)
{
    public static CappedExceptionAllocationView ToCappedExceptionAllocationView(CappedExceptionAllocation item)
    {
        return new CappedExceptionAllocationView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedExceptionAllocationView> ToCappedExceptionAllocationViews(List<CappedExceptionAllocation> items)
    {
        var views = new List<CappedExceptionAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToCappedExceptionAllocationView(item));
        }
        return views;
    }

    public static double SumCappedExceptionAllocationView(CappedExceptionAllocationView view)
    {
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedTenureMonths + parsedFte;
    }
}
