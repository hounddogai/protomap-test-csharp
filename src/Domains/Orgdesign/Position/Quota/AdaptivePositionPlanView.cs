namespace Acme.Workforce.Orgdesign.Position.Quota;

using System.Globalization;

/// <summary>How screens and exports show a adaptive position plan.</summary>
public sealed record AdaptivePositionPlanView(string Code, string Name, string QuantityText, string Version, string FloorValueText, string Pinned, string LimitText, string ThresholdText, string ScoreText, string RetriesText, string StatusLabel)
{
    public static AdaptivePositionPlanView ToAdaptivePositionPlanView(AdaptivePositionPlan item)
    {
        return new AdaptivePositionPlanView(
            item.Code,
            item.Name,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptivePositionPlanView> ToAdaptivePositionPlanViews(List<AdaptivePositionPlan> items)
    {
        var views = new List<AdaptivePositionPlanView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptivePositionPlanView(item));
        }
        return views;
    }

    public static double SumAdaptivePositionPlanView(AdaptivePositionPlanView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedVersion + parsedFloorValue;
    }
}
