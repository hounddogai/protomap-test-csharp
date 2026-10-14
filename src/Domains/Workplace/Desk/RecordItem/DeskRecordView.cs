namespace Acme.Workforce.Workplace.Desk.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a desk record.</summary>
public sealed record DeskRecordView(string Code, string TitleText, string OwnerId, string Source, string Discount, string TimezoneText, string Hours, string StatusLabel)
{
    public static DeskRecordView ToDeskRecordView(DeskRecord item)
    {
        return new DeskRecordView(
            item.Code,
            item.Title,
            Text.Truncate(item.OwnerId, 80),
            Text.Truncate(item.Source, 40),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskRecordView> ToDeskRecordViews(List<DeskRecord> items)
    {
        var views = new List<DeskRecordView>();
        foreach (var item in items)
        {
            views.Add(ToDeskRecordView(item));
        }
        return views;
    }

    public static double SumDeskRecordView(DeskRecordView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedHours;
    }
}
