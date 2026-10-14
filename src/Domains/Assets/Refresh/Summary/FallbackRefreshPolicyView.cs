namespace Acme.Workforce.Assets.Refresh.Summary;

using System.Globalization;

/// <summary>How screens and exports show a fallback refresh policy.</summary>
public sealed record FallbackRefreshPolicyView(string Code, string LabelText, string Source, string CostCenter, string Limit, string Archived, string StatusLabel)
{
    public static FallbackRefreshPolicyView ToFallbackRefreshPolicyView(FallbackRefreshPolicy item)
    {
        return new FallbackRefreshPolicyView(
            item.Code,
            item.Label,
            item.Source,
            item.CostCenter,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackRefreshPolicyView> ToFallbackRefreshPolicyViews(List<FallbackRefreshPolicy> items)
    {
        var views = new List<FallbackRefreshPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackRefreshPolicyView(item));
        }
        return views;
    }

    public static double SumFallbackRefreshPolicyView(FallbackRefreshPolicyView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedLimit;
    }
}
