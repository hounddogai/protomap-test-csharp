namespace Acme.Workforce.Compensation.MeritCycle.CycleTarget;

using System.Globalization;

/// <summary>How screens and exports show a lenient merit cycle target.</summary>
public sealed record LenientMeritCycleTargetView(string Code, string NameText, string WeightText, string Limit, string CostCenter, string FloorValue, string StatusLabel)
{
    public static LenientMeritCycleTargetView ToLenientMeritCycleTargetView(LenientMeritCycleTarget item)
    {
        return new LenientMeritCycleTargetView(
            item.Code,
            item.Name,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientMeritCycleTargetView> ToLenientMeritCycleTargetViews(List<LenientMeritCycleTarget> items)
    {
        var views = new List<LenientMeritCycleTargetView>();
        foreach (var item in items)
        {
            views.Add(ToLenientMeritCycleTargetView(item));
        }
        return views;
    }

    public static double SumLenientMeritCycleTargetView(LenientMeritCycleTargetView view)
    {
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedLimit + parsedFloorValue;
    }
}
