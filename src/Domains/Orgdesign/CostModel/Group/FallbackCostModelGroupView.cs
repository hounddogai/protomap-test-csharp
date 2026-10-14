namespace Acme.Workforce.Orgdesign.CostModel.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback cost model group.</summary>
public sealed record FallbackCostModelGroupView(string Code, string Title, string Weight, string DurationMinutes, string Retries, string Approved, string Quantity, string UpdatedAt, string OwnerId, string StatusLabel)
{
    public static FallbackCostModelGroupView ToFallbackCostModelGroupView(FallbackCostModelGroup item)
    {
        return new FallbackCostModelGroupView(
            item.Code,
            item.Title,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackCostModelGroupView> ToFallbackCostModelGroupViews(List<FallbackCostModelGroup> items)
    {
        var views = new List<FallbackCostModelGroupView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackCostModelGroupView(item));
        }
        return views;
    }

    public static double SumFallbackCostModelGroupView(FallbackCostModelGroupView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedDurationMinutes + (double)parsedRetries;
    }
}
