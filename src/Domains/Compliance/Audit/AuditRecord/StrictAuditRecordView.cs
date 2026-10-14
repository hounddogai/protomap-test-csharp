namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using System.Globalization;

/// <summary>How screens and exports show a strict audit record.</summary>
public sealed record StrictAuditRecordView(string CodeText, string TitleText, string SalaryBand, string Capacity, string WeightText, string Hours, string Cost, string ActiveText, string StatusLabel)
{
    public static StrictAuditRecordView ToStrictAuditRecordView(StrictAuditRecord item)
    {
        return new StrictAuditRecordView(
            item.Code,
            item.Title,
            item.SalaryBand,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictAuditRecordView> ToStrictAuditRecordViews(List<StrictAuditRecord> items)
    {
        var views = new List<StrictAuditRecordView>();
        foreach (var item in items)
        {
            views.Add(ToStrictAuditRecordView(item));
        }
        return views;
    }

    public static double SumStrictAuditRecordView(StrictAuditRecordView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedWeight + parsedHours;
    }
}
