namespace Acme.Workforce.Workplace.Visitor.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor claim.</summary>
public sealed record VisitorClaimView(string CodeText, string Label, string Rate, string UnitText, string DiscountText, string JobCode, string CreatedAt, string StatusLabel)
{
    public static VisitorClaimView ToVisitorClaimView(VisitorClaim item)
    {
        return new VisitorClaimView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 40),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorClaimView> ToVisitorClaimViews(List<VisitorClaim> items)
    {
        var views = new List<VisitorClaimView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorClaimView(item));
        }
        return views;
    }

    public static double SumVisitorClaimView(VisitorClaimView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return parsedRate + parsedDiscount + (double)parsedCreatedAt;
    }
}
