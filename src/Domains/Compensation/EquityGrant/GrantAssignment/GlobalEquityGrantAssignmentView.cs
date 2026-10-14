namespace Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global equity grant assignment.</summary>
public sealed record GlobalEquityGrantAssignmentView(string CodeText, string Label, string Grade, string ContactEmail, string Quantity, string ChecksumText, string Weight, string Score, string Discount, string SalaryBand, string Phone, string StatusLabel)
{
    public static GlobalEquityGrantAssignmentView ToGlobalEquityGrantAssignmentView(GlobalEquityGrantAssignment item)
    {
        return new GlobalEquityGrantAssignmentView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Grade,
            item.ContactEmail,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 120),
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalEquityGrantAssignmentView> ToGlobalEquityGrantAssignmentViews(List<GlobalEquityGrantAssignment> items)
    {
        var views = new List<GlobalEquityGrantAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalEquityGrantAssignmentView(item));
        }
        return views;
    }

    public static double SumGlobalEquityGrantAssignmentView(GlobalEquityGrantAssignmentView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedWeight + parsedScore;
    }
}
