namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using System.Globalization;

/// <summary>How screens and exports show a retro pay threshold.</summary>
public sealed record RetroPayThresholdView(string Code, string Label, string Ceiling, string Location, string UpdatedAt, string ArchivedText, string Cost, string StatusLabel)
{
    public static RetroPayThresholdView ToRetroPayThresholdView(RetroPayThreshold item)
    {
        return new RetroPayThresholdView(
            item.Code,
            item.Label,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayThresholdView> ToRetroPayThresholdViews(List<RetroPayThreshold> items)
    {
        var views = new List<RetroPayThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayThresholdView(item));
        }
        return views;
    }

    public static double SumRetroPayThresholdView(RetroPayThresholdView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedUpdatedAt + parsedCost;
    }
}
