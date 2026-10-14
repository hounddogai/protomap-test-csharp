namespace Acme.Workforce.Compensation.BonusPool.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool schedule.</summary>
public sealed record BonusPoolScheduleView(string CodeText, string Title, string Locale, string Amount, string Limit, string StatusLabel)
{
    public static BonusPoolScheduleView ToBonusPoolScheduleView(BonusPoolSchedule item)
    {
        return new BonusPoolScheduleView(
            item.Code,
            Text.Truncate(item.Title, 40),
            Text.Truncate(item.Locale, 120),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolScheduleView> ToBonusPoolScheduleViews(List<BonusPoolSchedule> items)
    {
        var views = new List<BonusPoolScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolScheduleView(item));
        }
        return views;
    }

    public static double SumBonusPoolScheduleView(BonusPoolScheduleView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedLimit;
    }
}
