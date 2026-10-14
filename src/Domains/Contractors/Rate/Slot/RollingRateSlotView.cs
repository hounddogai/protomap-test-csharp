namespace Acme.Workforce.Contractors.Rate.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling rate slot.</summary>
public sealed record RollingRateSlotView(string Code, string Label, string RatioText, string AttemptsText, string LimitText, string ManagerId, string StatusLabel)
{
    public static RollingRateSlotView ToRollingRateSlotView(RollingRateSlot item)
    {
        return new RollingRateSlotView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Label, 64),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingRateSlotView> ToRollingRateSlotViews(List<RollingRateSlot> items)
    {
        var views = new List<RollingRateSlotView>();
        foreach (var item in items)
        {
            views.Add(ToRollingRateSlotView(item));
        }
        return views;
    }

    public static double SumRollingRateSlotView(RollingRateSlotView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedAttempts + (double)parsedLimit;
    }
}
