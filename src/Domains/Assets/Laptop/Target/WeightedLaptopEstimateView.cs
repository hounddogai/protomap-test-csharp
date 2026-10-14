namespace Acme.Workforce.Assets.Laptop.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted laptop estimate.</summary>
public sealed record WeightedLaptopEstimateView(string Code, string TitleText, string Unit, string Checksum, string JobCodeText, string TimezoneText, string WeightText, string Country, string Ceiling, string StatusLabel)
{
    public static WeightedLaptopEstimateView ToWeightedLaptopEstimateView(WeightedLaptopEstimate item)
    {
        return new WeightedLaptopEstimateView(
            item.Code,
            item.Title,
            item.Unit,
            item.Checksum,
            item.JobCode,
            Text.Truncate(item.Timezone, 120),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedLaptopEstimateView> ToWeightedLaptopEstimateViews(List<WeightedLaptopEstimate> items)
    {
        var views = new List<WeightedLaptopEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedLaptopEstimateView(item));
        }
        return views;
    }

    public static double SumWeightedLaptopEstimateView(WeightedLaptopEstimateView view)
    {
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedWeight + parsedCeiling;
    }
}
