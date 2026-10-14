namespace Acme.Workforce.Workplace.Desk.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted desk preference.</summary>
public sealed record WeightedDeskPreferenceView(string Code, string LabelText, string SalaryBand, string RegionText, string Department, string OwnerId, string TenureMonthsText, string Sequence, string Weight, string Timezone, string StatusLabel)
{
    public static WeightedDeskPreferenceView ToWeightedDeskPreferenceView(WeightedDeskPreference item)
    {
        return new WeightedDeskPreferenceView(
            item.Code,
            item.Label,
            Text.Truncate(item.SalaryBand, 64),
            Text.Truncate(item.Region, 80),
            Text.Truncate(item.Department, 64),
            item.OwnerId,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedDeskPreferenceView> ToWeightedDeskPreferenceViews(List<WeightedDeskPreference> items)
    {
        var views = new List<WeightedDeskPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedDeskPreferenceView(item));
        }
        return views;
    }

    public static double SumWeightedDeskPreferenceView(WeightedDeskPreferenceView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedSequence + parsedWeight;
    }
}
