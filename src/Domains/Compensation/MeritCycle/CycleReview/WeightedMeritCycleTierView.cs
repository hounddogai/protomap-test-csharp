namespace Acme.Workforce.Compensation.MeritCycle.CycleReview;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted merit cycle tier.</summary>
public sealed record WeightedMeritCycleTierView(string Code, string Label, string Maximum, string ContactEmail, string FloorValueText, string StatusLabel)
{
    public static WeightedMeritCycleTierView ToWeightedMeritCycleTierView(WeightedMeritCycleTier item)
    {
        return new WeightedMeritCycleTierView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedMeritCycleTierView> ToWeightedMeritCycleTierViews(List<WeightedMeritCycleTier> items)
    {
        var views = new List<WeightedMeritCycleTierView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedMeritCycleTierView(item));
        }
        return views;
    }

    public static double SumWeightedMeritCycleTierView(WeightedMeritCycleTierView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedFloorValue;
    }
}
