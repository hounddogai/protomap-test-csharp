namespace Acme.Workforce.Workplace.Visitor.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional visitor baseline.</summary>
public sealed record RegionalVisitorBaselineView(string Code, string Title, string ManagerId, string RevisionText, string Phone, string Badge, string WindowDaysText, string SalaryBandText, string Discount, string SequenceText, string StatusLabel)
{
    public static RegionalVisitorBaselineView ToRegionalVisitorBaselineView(RegionalVisitorBaseline item)
    {
        return new RegionalVisitorBaselineView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.ManagerId,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.Badge,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 64),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalVisitorBaselineView> ToRegionalVisitorBaselineViews(List<RegionalVisitorBaseline> items)
    {
        var views = new List<RegionalVisitorBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalVisitorBaselineView(item));
        }
        return views;
    }

    public static double SumRegionalVisitorBaselineView(RegionalVisitorBaselineView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedWindowDays + parsedDiscount;
    }
}
