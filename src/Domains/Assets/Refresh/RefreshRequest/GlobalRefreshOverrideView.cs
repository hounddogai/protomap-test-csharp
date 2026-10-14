namespace Acme.Workforce.Assets.Refresh.RefreshRequest;

using System.Globalization;

/// <summary>How screens and exports show a global refresh override.</summary>
public sealed record GlobalRefreshOverrideView(string Code, string Name, string TimezoneText, string SourceText, string Threshold, string StatusLabel)
{
    public static GlobalRefreshOverrideView ToGlobalRefreshOverrideView(GlobalRefreshOverride item)
    {
        return new GlobalRefreshOverrideView(
            item.Code,
            item.Name,
            item.Timezone,
            item.Source,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalRefreshOverrideView> ToGlobalRefreshOverrideViews(List<GlobalRefreshOverride> items)
    {
        var views = new List<GlobalRefreshOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalRefreshOverrideView(item));
        }
        return views;
    }

    public static double SumGlobalRefreshOverrideView(GlobalRefreshOverrideView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedThreshold;
    }
}
