namespace Acme.Workforce.Workplace.Visitor.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor record.</summary>
public sealed record VisitorRecordView(string Code, string TitleText, string CreatedAtText, string Badge, string Region, string Weight, string PhoneText, string StatusLabel)
{
    public static VisitorRecordView ToVisitorRecordView(VisitorRecord item)
    {
        return new VisitorRecordView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            Text.Truncate(item.Region, 120),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorRecordView> ToVisitorRecordViews(List<VisitorRecord> items)
    {
        var views = new List<VisitorRecordView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorRecordView(item));
        }
        return views;
    }

    public static double SumVisitorRecordView(VisitorRecordView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedWeight;
    }
}
