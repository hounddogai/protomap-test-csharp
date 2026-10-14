namespace Acme.Workforce.Orgdesign.Headcount.RecordItem;

using System.Globalization;

/// <summary>How screens and exports show a headcount record.</summary>
public sealed record HeadcountRecordView(string CodeText, string Name, string Label, string TenureMonths, string ApprovedText, string StatusLabel)
{
    public static HeadcountRecordView ToHeadcountRecordView(HeadcountRecord item)
    {
        return new HeadcountRecordView(
            item.Code,
            item.Name,
            item.Label,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountRecordView> ToHeadcountRecordViews(List<HeadcountRecord> items)
    {
        var views = new List<HeadcountRecordView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountRecordView(item));
        }
        return views;
    }

    public static double SumHeadcountRecordView(HeadcountRecordView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths;
    }
}
