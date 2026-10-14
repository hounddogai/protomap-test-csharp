namespace Acme.Workforce.Orgdesign.Position.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient position profile.</summary>
public sealed record LenientPositionProfileView(string Code, string TitleText, string ThresholdText, string Badge, string Unit, string CostText, string NameText, string JobCodeText, string StatusLabel)
{
    public static LenientPositionProfileView ToLenientPositionProfileView(LenientPositionProfile item)
    {
        return new LenientPositionProfileView(
            item.Code,
            item.Title,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.Unit,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 120),
            Text.Truncate(item.JobCode, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientPositionProfileView> ToLenientPositionProfileViews(List<LenientPositionProfile> items)
    {
        var views = new List<LenientPositionProfileView>();
        foreach (var item in items)
        {
            views.Add(ToLenientPositionProfileView(item));
        }
        return views;
    }

    public static double SumLenientPositionProfileView(LenientPositionProfileView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedCost;
    }
}
