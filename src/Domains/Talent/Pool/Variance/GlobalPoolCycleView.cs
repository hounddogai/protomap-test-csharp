namespace Acme.Workforce.Talent.Pool.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global pool cycle.</summary>
public sealed record GlobalPoolCycleView(string Code, string Name, string EmployeeId, string Timezone, string Weight, string Rating, string UnitText, string Checksum, string Approved, string StatusLabel)
{
    public static GlobalPoolCycleView ToGlobalPoolCycleView(GlobalPoolCycle item)
    {
        return new GlobalPoolCycleView(
            item.Code,
            item.Name,
            Text.Truncate(item.EmployeeId, 40),
            item.Timezone,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            item.Checksum,
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalPoolCycleView> ToGlobalPoolCycleViews(List<GlobalPoolCycle> items)
    {
        var views = new List<GlobalPoolCycleView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalPoolCycleView(item));
        }
        return views;
    }

    public static double SumGlobalPoolCycleView(GlobalPoolCycleView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedWeight + parsedRating;
    }
}
