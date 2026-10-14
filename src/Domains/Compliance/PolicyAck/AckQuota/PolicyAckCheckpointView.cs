namespace Acme.Workforce.Compliance.PolicyAck.AckQuota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy ack checkpoint.</summary>
public sealed record PolicyAckCheckpointView(string CodeText, string Title, string Margin, string Revision, string Location, string ReviewerText, string Category, string Channel, string Unit, string Ceiling, string StatusLabel)
{
    public static PolicyAckCheckpointView ToPolicyAckCheckpointView(PolicyAckCheckpoint item)
    {
        return new PolicyAckCheckpointView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.Reviewer,
            Text.Truncate(item.Category, 64),
            item.Channel,
            item.Unit,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckCheckpointView> ToPolicyAckCheckpointViews(List<PolicyAckCheckpoint> items)
    {
        var views = new List<PolicyAckCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckCheckpointView(item));
        }
        return views;
    }

    public static double SumPolicyAckCheckpointView(PolicyAckCheckpointView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedRevision + parsedCeiling;
    }
}
