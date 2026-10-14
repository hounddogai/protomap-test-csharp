namespace Acme.Workforce.Assets.Laptop.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a laptop record.</summary>
public sealed record LaptopRecordView(string Code, string LabelText, string Checksum, string Timezone, string Reviewer, string Weight, string Region, string StartsAt, string StatusLabel)
{
    public static LaptopRecordView ToLaptopRecordView(LaptopRecord item)
    {
        return new LaptopRecordView(
            item.Code,
            item.Label,
            item.Checksum,
            item.Timezone,
            Text.Truncate(item.Reviewer, 64),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 80),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopRecordView> ToLaptopRecordViews(List<LaptopRecord> items)
    {
        var views = new List<LaptopRecordView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopRecordView(item));
        }
        return views;
    }

    public static double SumLaptopRecordView(LaptopRecordView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedStartsAt;
    }
}
