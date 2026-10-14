namespace Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy position target.</summary>
public sealed record LegacyPositionTargetView(string Code, string Title, string QuantityText, string LocationText, string Unit, string Threshold, string StatusLabel)
{
    public static LegacyPositionTargetView ToLegacyPositionTargetView(LegacyPositionTarget item)
    {
        return new LegacyPositionTargetView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 120),
            item.Unit,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyPositionTargetView> ToLegacyPositionTargetViews(List<LegacyPositionTarget> items)
    {
        var views = new List<LegacyPositionTargetView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyPositionTargetView(item));
        }
        return views;
    }

    public static double SumLegacyPositionTargetView(LegacyPositionTargetView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedThreshold;
    }
}
