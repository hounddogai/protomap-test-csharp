namespace Acme.Workforce.Compensation.MeritCycle.CycleSegment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted merit cycle bundle.</summary>
public sealed record WeightedMeritCycleBundleView(string Code, string Label, string Sequence, string TenureMonths, string CountryText, string Retries, string Budget, string Pinned, string Margin, string FloorValue, string StatusLabel)
{
    public static WeightedMeritCycleBundleView ToWeightedMeritCycleBundleView(WeightedMeritCycleBundle item)
    {
        return new WeightedMeritCycleBundleView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Country,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedMeritCycleBundleView> ToWeightedMeritCycleBundleViews(List<WeightedMeritCycleBundle> items)
    {
        var views = new List<WeightedMeritCycleBundleView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedMeritCycleBundleView(item));
        }
        return views;
    }

    public static double SumWeightedMeritCycleBundleView(WeightedMeritCycleBundleView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedTenureMonths + (double)parsedRetries;
    }
}
