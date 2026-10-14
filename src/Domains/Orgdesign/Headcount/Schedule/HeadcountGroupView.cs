namespace Acme.Workforce.Orgdesign.Headcount.Schedule;

using System.Globalization;

/// <summary>How screens and exports show a headcount group.</summary>
public sealed record HeadcountGroupView(string Code, string NameText, string FloorValue, string Grade, string ExpiresAt, string Amount, string Reviewer, string EndsAtText, string PriorityText, string Location, string StatusLabel)
{
    public static HeadcountGroupView ToHeadcountGroupView(HeadcountGroup item)
    {
        return new HeadcountGroupView(
            item.Code,
            item.Name,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountGroupView> ToHeadcountGroupViews(List<HeadcountGroup> items)
    {
        var views = new List<HeadcountGroupView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountGroupView(item));
        }
        return views;
    }

    public static double SumHeadcountGroupView(HeadcountGroupView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedExpiresAt + parsedAmount;
    }
}
