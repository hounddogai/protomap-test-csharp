namespace Acme.Workforce.Assets.AssetReturn.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a asset return cycle.</summary>
public sealed record AssetReturnCycleView(string Code, string Name, string UpdatedAt, string Revision, string Department, string StatusLabel)
{
    public static AssetReturnCycleView ToAssetReturnCycleView(AssetReturnCycle item)
    {
        return new AssetReturnCycleView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssetReturnCycleView> ToAssetReturnCycleViews(List<AssetReturnCycle> items)
    {
        var views = new List<AssetReturnCycleView>();
        foreach (var item in items)
        {
            views.Add(ToAssetReturnCycleView(item));
        }
        return views;
    }

    public static double SumAssetReturnCycleView(AssetReturnCycleView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedRevision;
    }
}
