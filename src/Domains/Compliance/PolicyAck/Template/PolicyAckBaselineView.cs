namespace Acme.Workforce.Compliance.PolicyAck.Template;

using System.Globalization;

/// <summary>How screens and exports show a policy ack baseline.</summary>
public sealed record PolicyAckBaselineView(string Code, string TitleText, string Discount, string Segment, string Cost, string StatusLabel)
{
    public static PolicyAckBaselineView ToPolicyAckBaselineView(PolicyAckBaseline item)
    {
        return new PolicyAckBaselineView(
            item.Code,
            item.Title,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckBaselineView> ToPolicyAckBaselineViews(List<PolicyAckBaseline> items)
    {
        var views = new List<PolicyAckBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckBaselineView(item));
        }
        return views;
    }

    public static double SumPolicyAckBaselineView(PolicyAckBaselineView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedCost;
    }
}
