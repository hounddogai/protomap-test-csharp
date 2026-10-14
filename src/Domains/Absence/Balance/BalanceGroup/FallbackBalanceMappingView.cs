namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback balance mapping.</summary>
public sealed record FallbackBalanceMappingView(string Code, string Title, string UpdatedAtText, string Minimum, string OwnerId, string ExternalRef, string CategoryText, string Ceiling, string StatusLabel)
{
    public static FallbackBalanceMappingView ToFallbackBalanceMappingView(FallbackBalanceMapping item)
    {
        return new FallbackBalanceMappingView(
            item.Code,
            item.Title,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            Text.Truncate(item.ExternalRef, 80),
            item.Category,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackBalanceMappingView> ToFallbackBalanceMappingViews(List<FallbackBalanceMapping> items)
    {
        var views = new List<FallbackBalanceMappingView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackBalanceMappingView(item));
        }
        return views;
    }

    public static double SumFallbackBalanceMappingView(FallbackBalanceMappingView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedMinimum + parsedCeiling;
    }
}
