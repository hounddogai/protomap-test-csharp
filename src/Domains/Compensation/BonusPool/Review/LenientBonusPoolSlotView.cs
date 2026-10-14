namespace Acme.Workforce.Compensation.BonusPool.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient bonus pool slot.</summary>
public sealed record LenientBonusPoolSlotView(string CodeText, string TitleText, string Hours, string FloorValueText, string Department, string Rating, string Revision, string Grade, string Threshold, string EmployeeId, string StartsAtText, string StatusLabel)
{
    public static LenientBonusPoolSlotView ToLenientBonusPoolSlotView(LenientBonusPoolSlot item)
    {
        return new LenientBonusPoolSlotView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 120),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientBonusPoolSlotView> ToLenientBonusPoolSlotViews(List<LenientBonusPoolSlot> items)
    {
        var views = new List<LenientBonusPoolSlotView>();
        foreach (var item in items)
        {
            views.Add(ToLenientBonusPoolSlotView(item));
        }
        return views;
    }

    public static double SumLenientBonusPoolSlotView(LenientBonusPoolSlotView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedHours + parsedFloorValue + parsedRating;
    }
}
