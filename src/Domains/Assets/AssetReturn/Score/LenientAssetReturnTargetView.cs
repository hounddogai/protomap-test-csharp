namespace Acme.Workforce.Assets.AssetReturn.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient asset return target.</summary>
public sealed record LenientAssetReturnTargetView(string Code, string Name, string DurationMinutes, string Reviewer, string Channel, string StatusLabel)
{
    public static LenientAssetReturnTargetView ToLenientAssetReturnTargetView(LenientAssetReturnTarget item)
    {
        return new LenientAssetReturnTargetView(
            item.Code,
            item.Name,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            Text.Truncate(item.Channel, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientAssetReturnTargetView> ToLenientAssetReturnTargetViews(List<LenientAssetReturnTarget> items)
    {
        var views = new List<LenientAssetReturnTargetView>();
        foreach (var item in items)
        {
            views.Add(ToLenientAssetReturnTargetView(item));
        }
        return views;
    }

    public static double SumLenientAssetReturnTargetView(LenientAssetReturnTargetView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes;
    }
}
