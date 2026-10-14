namespace Acme.Workforce.Contractors.Rate.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback rate allocation.</summary>
public sealed record FallbackRateAllocationView(string Code, string NameText, string Ceiling, string ExpiresAt, string Channel, string StatusLabel)
{
    public static FallbackRateAllocationView ToFallbackRateAllocationView(FallbackRateAllocation item)
    {
        return new FallbackRateAllocationView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackRateAllocationView> ToFallbackRateAllocationViews(List<FallbackRateAllocation> items)
    {
        var views = new List<FallbackRateAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackRateAllocationView(item));
        }
        return views;
    }

    public static double SumFallbackRateAllocationView(FallbackRateAllocationView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedExpiresAt;
    }
}
