namespace Acme.Workforce.Compliance.PolicyAck.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered policy ack variance.</summary>
public sealed record TieredPolicyAckVarianceView(string Code, string Label, string Notes, string SourceText, string VersionText, string LocationText, string OwnerId, string ArchivedText, string ManagerId, string CostCenterText, string StatusLabel)
{
    public static TieredPolicyAckVarianceView ToTieredPolicyAckVarianceView(TieredPolicyAckVariance item)
    {
        return new TieredPolicyAckVarianceView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Notes,
            Text.Truncate(item.Source, 40),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.OwnerId,
            $"{item.Archived}",
            item.ManagerId,
            Text.Truncate(item.CostCenter, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredPolicyAckVarianceView> ToTieredPolicyAckVarianceViews(List<TieredPolicyAckVariance> items)
    {
        var views = new List<TieredPolicyAckVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToTieredPolicyAckVarianceView(item));
        }
        return views;
    }

    public static double SumTieredPolicyAckVarianceView(TieredPolicyAckVarianceView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        return (double)parsedVersion;
    }
}
