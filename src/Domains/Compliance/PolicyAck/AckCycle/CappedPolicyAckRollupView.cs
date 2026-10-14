namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped policy ack rollup.</summary>
public sealed record CappedPolicyAckRollupView(string Code, string Title, string Maximum, string Segment, string Minimum, string Summary, string StatusLabel)
{
    public static CappedPolicyAckRollupView ToCappedPolicyAckRollupView(CappedPolicyAckRollup item)
    {
        return new CappedPolicyAckRollupView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 64),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedPolicyAckRollupView> ToCappedPolicyAckRollupViews(List<CappedPolicyAckRollup> items)
    {
        var views = new List<CappedPolicyAckRollupView>();
        foreach (var item in items)
        {
            views.Add(ToCappedPolicyAckRollupView(item));
        }
        return views;
    }

    public static double SumCappedPolicyAckRollupView(CappedPolicyAckRollupView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedMinimum;
    }
}
