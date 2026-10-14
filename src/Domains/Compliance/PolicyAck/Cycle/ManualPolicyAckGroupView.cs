namespace Acme.Workforce.Compliance.PolicyAck.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual policy ack group.</summary>
public sealed record ManualPolicyAckGroupView(string Code, string Name, string Margin, string Hours, string Version, string OwnerId, string TimezoneText, string ManagerId, string ContactEmailText, string Minimum, string StatusLabel)
{
    public static ManualPolicyAckGroupView ToManualPolicyAckGroupView(ManualPolicyAckGroup item)
    {
        return new ManualPolicyAckGroupView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            Text.Truncate(item.Timezone, 40),
            item.ManagerId,
            item.ContactEmail,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualPolicyAckGroupView> ToManualPolicyAckGroupViews(List<ManualPolicyAckGroup> items)
    {
        var views = new List<ManualPolicyAckGroupView>();
        foreach (var item in items)
        {
            views.Add(ToManualPolicyAckGroupView(item));
        }
        return views;
    }

    public static double SumManualPolicyAckGroupView(ManualPolicyAckGroupView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedMargin + parsedHours + (double)parsedVersion;
    }
}
