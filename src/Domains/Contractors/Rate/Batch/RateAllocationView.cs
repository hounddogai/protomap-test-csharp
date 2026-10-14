namespace Acme.Workforce.Contractors.Rate.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rate allocation.</summary>
public sealed record RateAllocationView(string Code, string Label, string ChannelText, string LeaveDays, string Source, string Revision, string StatusLabel)
{
    public static RateAllocationView ToRateAllocationView(RateAllocation item)
    {
        return new RateAllocationView(
            item.Code,
            Text.Truncate(item.Label, 40),
            Text.Truncate(item.Channel, 40),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 64),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateAllocationView> ToRateAllocationViews(List<RateAllocation> items)
    {
        var views = new List<RateAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToRateAllocationView(item));
        }
        return views;
    }

    public static double SumRateAllocationView(RateAllocationView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedRevision;
    }
}
