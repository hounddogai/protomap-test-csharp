namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using System.Globalization;

/// <summary>How screens and exports show a global policy ack bundle.</summary>
public sealed record GlobalPolicyAckBundleView(string Code, string TitleText, string Threshold, string GraceDays, string JobCodeText, string UpdatedAt, string CreatedAtText, string CostCenter, string OwnerId, string StatusLabel)
{
    public static GlobalPolicyAckBundleView ToGlobalPolicyAckBundleView(GlobalPolicyAckBundle item)
    {
        return new GlobalPolicyAckBundleView(
            item.Code,
            item.Title,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalPolicyAckBundleView> ToGlobalPolicyAckBundleViews(List<GlobalPolicyAckBundle> items)
    {
        var views = new List<GlobalPolicyAckBundleView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalPolicyAckBundleView(item));
        }
        return views;
    }

    public static double SumGlobalPolicyAckBundleView(GlobalPolicyAckBundleView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedGraceDays + (double)parsedUpdatedAt;
    }
}
