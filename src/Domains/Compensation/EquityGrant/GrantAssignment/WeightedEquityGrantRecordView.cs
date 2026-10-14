namespace Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted equity grant record.</summary>
public sealed record WeightedEquityGrantRecordView(string Code, string Name, string ExternalRef, string EndsAtText, string ChannelText, string Phone, string ExpiresAt, string Department, string Discount, string ReviewerText, string StatusLabel)
{
    public static WeightedEquityGrantRecordView ToWeightedEquityGrantRecordView(WeightedEquityGrantRecord item)
    {
        return new WeightedEquityGrantRecordView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.ExternalRef,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Phone,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 40),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedEquityGrantRecordView> ToWeightedEquityGrantRecordViews(List<WeightedEquityGrantRecord> items)
    {
        var views = new List<WeightedEquityGrantRecordView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedEquityGrantRecordView(item));
        }
        return views;
    }

    public static double SumWeightedEquityGrantRecordView(WeightedEquityGrantRecordView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedExpiresAt + parsedDiscount;
    }
}
