namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending desk allocation.</summary>
public sealed record PendingDeskAllocationView(string Code, string LabelText, string Team, string Badge, string EndsAt, string ChannelText, string Location, string RetriesText, string CostCenter, string StatusLabel)
{
    public static PendingDeskAllocationView ToPendingDeskAllocationView(PendingDeskAllocation item)
    {
        return new PendingDeskAllocationView(
            item.Code,
            item.Label,
            Text.Truncate(item.Team, 40),
            item.Badge,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 64),
            item.Location,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.CostCenter, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingDeskAllocationView> ToPendingDeskAllocationViews(List<PendingDeskAllocation> items)
    {
        var views = new List<PendingDeskAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToPendingDeskAllocationView(item));
        }
        return views;
    }

    public static double SumPendingDeskAllocationView(PendingDeskAllocationView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedRetries;
    }
}
