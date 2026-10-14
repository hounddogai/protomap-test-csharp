namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling payslip digest.</summary>
public sealed record RollingPayslipDigestView(string Code, string Label, string Badge, string Version, string Name, string Maximum, string OwnerId, string Visible, string JobCodeText, string ChannelText, string StatusLabel)
{
    public static RollingPayslipDigestView ToRollingPayslipDigestView(RollingPayslipDigest item)
    {
        return new RollingPayslipDigestView(
            item.Code,
            item.Label,
            item.Badge,
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 120),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Visible}",
            item.JobCode,
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingPayslipDigestView> ToRollingPayslipDigestViews(List<RollingPayslipDigest> items)
    {
        var views = new List<RollingPayslipDigestView>();
        foreach (var item in items)
        {
            views.Add(ToRollingPayslipDigestView(item));
        }
        return views;
    }

    public static double SumRollingPayslipDigestView(RollingPayslipDigestView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedMaximum;
    }
}
