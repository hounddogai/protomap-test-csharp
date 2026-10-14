namespace Acme.Workforce.Orgdesign.CostModel.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted cost model slot.</summary>
public sealed record WeightedCostModelSlotView(string Code, string NameText, string Country, string BatchSizeText, string RetriesText, string Timezone, string StatusLabel)
{
    public static WeightedCostModelSlotView ToWeightedCostModelSlotView(WeightedCostModelSlot item)
    {
        return new WeightedCostModelSlotView(
            item.Code,
            item.Name,
            Text.Truncate(item.Country, 40),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedCostModelSlotView> ToWeightedCostModelSlotViews(List<WeightedCostModelSlot> items)
    {
        var views = new List<WeightedCostModelSlotView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedCostModelSlotView(item));
        }
        return views;
    }

    public static double SumWeightedCostModelSlotView(WeightedCostModelSlotView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedRetries;
    }
}
