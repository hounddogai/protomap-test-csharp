namespace Acme.Workforce.Assets.AssetReturn.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary asset return profile.</summary>
public sealed record PrimaryAssetReturnProfileView(string Code, string Title, string CategoryText, string VisibleText, string Locale, string LeaveDays, string StatusLabel)
{
    public static PrimaryAssetReturnProfileView ToPrimaryAssetReturnProfileView(PrimaryAssetReturnProfile item)
    {
        return new PrimaryAssetReturnProfileView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Category,
            $"{item.Visible}",
            item.Locale,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryAssetReturnProfileView> ToPrimaryAssetReturnProfileViews(List<PrimaryAssetReturnProfile> items)
    {
        var views = new List<PrimaryAssetReturnProfileView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryAssetReturnProfileView(item));
        }
        return views;
    }

    public static double SumPrimaryAssetReturnProfileView(PrimaryAssetReturnProfileView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays;
    }
}
