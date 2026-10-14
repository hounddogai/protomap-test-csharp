namespace Acme.Workforce.Absence.Holiday.HolidayPolicy;

using System.Globalization;

/// <summary>How screens and exports show a manual holiday cycle.</summary>
public sealed record ManualHolidayCycleView(string Code, string LabelText, string EndsAt, string Discount, string RevisionText, string Rating, string ExpiresAtText, string Reviewer, string StatusLabel)
{
    public static ManualHolidayCycleView ToManualHolidayCycleView(ManualHolidayCycle item)
    {
        return new ManualHolidayCycleView(
            item.Code,
            item.Label,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualHolidayCycleView> ToManualHolidayCycleViews(List<ManualHolidayCycle> items)
    {
        var views = new List<ManualHolidayCycleView>();
        foreach (var item in items)
        {
            views.Add(ToManualHolidayCycleView(item));
        }
        return views;
    }

    public static double SumManualHolidayCycleView(ManualHolidayCycleView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedDiscount + (double)parsedRevision;
    }
}
