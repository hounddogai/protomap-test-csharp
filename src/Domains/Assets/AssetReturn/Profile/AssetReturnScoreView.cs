namespace Acme.Workforce.Assets.AssetReturn.Profile;

using System.Globalization;

/// <summary>How screens and exports show a asset return score.</summary>
public sealed record AssetReturnScoreView(string Code, string Name, string Active, string AttemptsText, string Timezone, string Skill, string StatusLabel)
{
    public static AssetReturnScoreView ToAssetReturnScoreView(AssetReturnScore item)
    {
        return new AssetReturnScoreView(
            item.Code,
            item.Name,
            $"{item.Active}",
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssetReturnScoreView> ToAssetReturnScoreViews(List<AssetReturnScore> items)
    {
        var views = new List<AssetReturnScoreView>();
        foreach (var item in items)
        {
            views.Add(ToAssetReturnScoreView(item));
        }
        return views;
    }

    public static double SumAssetReturnScoreView(AssetReturnScoreView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts;
    }
}
