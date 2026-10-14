namespace Acme.Workforce.Compensation.MeritCycle.CycleSegment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive merit cycle mapping.</summary>
public sealed record AdaptiveMeritCycleMappingView(string Code, string Name, string CreatedAt, string UpdatedAt, string Amount, string Timezone, string ThresholdText, string Location, string StatusLabel)
{
    public static AdaptiveMeritCycleMappingView ToAdaptiveMeritCycleMappingView(AdaptiveMeritCycleMapping item)
    {
        return new AdaptiveMeritCycleMappingView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveMeritCycleMappingView> ToAdaptiveMeritCycleMappingViews(List<AdaptiveMeritCycleMapping> items)
    {
        var views = new List<AdaptiveMeritCycleMappingView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveMeritCycleMappingView(item));
        }
        return views;
    }

    public static double SumAdaptiveMeritCycleMappingView(AdaptiveMeritCycleMappingView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedUpdatedAt + parsedAmount;
    }
}
