namespace Acme.Workforce.Absence.Accrual.AccrualAssignment;

using System.Globalization;

/// <summary>How screens and exports show a weighted accrual bundle.</summary>
public sealed record WeightedAccrualBundleView(string CodeText, string Name, string CapacityText, string BadgeText, string CeilingText, string StatusLabel)
{
    public static WeightedAccrualBundleView ToWeightedAccrualBundleView(WeightedAccrualBundle item)
    {
        return new WeightedAccrualBundleView(
            item.Code,
            item.Name,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedAccrualBundleView> ToWeightedAccrualBundleViews(List<WeightedAccrualBundle> items)
    {
        var views = new List<WeightedAccrualBundleView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedAccrualBundleView(item));
        }
        return views;
    }

    public static double SumWeightedAccrualBundleView(WeightedAccrualBundleView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedCeiling;
    }
}
