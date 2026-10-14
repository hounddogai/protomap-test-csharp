namespace Acme.Workforce.Contractors.Rate.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rate audit.</summary>
public sealed record RateAuditView(string Code, string Name, string PhoneText, string RateText, string DiscountText, string Approved, string Sequence, string OwnerId, string StatusLabel)
{
    public static RateAuditView ToRateAuditView(RateAudit item)
    {
        return new RateAuditView(
            item.Code,
            item.Name,
            Text.Truncate(item.Phone, 64),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateAuditView> ToRateAuditViews(List<RateAudit> items)
    {
        var views = new List<RateAuditView>();
        foreach (var item in items)
        {
            views.Add(ToRateAuditView(item));
        }
        return views;
    }

    public static double SumRateAuditView(RateAuditView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return parsedRate + parsedDiscount + (double)parsedSequence;
    }
}
