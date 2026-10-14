namespace Acme.Workforce.Engagement.ActionPlan.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient action plan record.</summary>
public sealed record LenientActionPlanRecordView(string Code, string Name, string Checksum, string Approved, string Phone, string Capacity, string HoursText, string Active, string Fte, string SequenceText, string Summary, string StatusLabel)
{
    public static LenientActionPlanRecordView ToLenientActionPlanRecordView(LenientActionPlanRecord item)
    {
        return new LenientActionPlanRecordView(
            Text.Truncate(item.Code, 80),
            item.Name,
            Text.Truncate(item.Checksum, 120),
            $"{item.Approved}",
            item.Phone,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientActionPlanRecordView> ToLenientActionPlanRecordViews(List<LenientActionPlanRecord> items)
    {
        var views = new List<LenientActionPlanRecordView>();
        foreach (var item in items)
        {
            views.Add(ToLenientActionPlanRecordView(item));
        }
        return views;
    }

    public static double SumLenientActionPlanRecordView(LenientActionPlanRecordView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedHours + parsedFte;
    }
}
