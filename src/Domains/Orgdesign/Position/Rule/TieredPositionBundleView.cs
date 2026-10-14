namespace Acme.Workforce.Orgdesign.Position.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered position bundle.</summary>
public sealed record TieredPositionBundleView(string Code, string NameText, string Segment, string EmployeeId, string Minimum, string StatusLabel)
{
    public static TieredPositionBundleView ToTieredPositionBundleView(TieredPositionBundle item)
    {
        return new TieredPositionBundleView(
            item.Code,
            item.Name,
            Text.Truncate(item.Segment, 120),
            Text.Truncate(item.EmployeeId, 40),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredPositionBundleView> ToTieredPositionBundleViews(List<TieredPositionBundle> items)
    {
        var views = new List<TieredPositionBundleView>();
        foreach (var item in items)
        {
            views.Add(ToTieredPositionBundleView(item));
        }
        return views;
    }

    public static double SumTieredPositionBundleView(TieredPositionBundleView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedMinimum;
    }
}
