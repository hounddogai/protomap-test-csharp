namespace Acme.Workforce.Talent.Mobility.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility audit.</summary>
public sealed record MobilityAuditView(string Code, string Title, string Weight, string Minimum, string Checksum, string TenureMonths, string ReviewerText, string CeilingText, string VersionText, string ManagerId, string StatusLabel)
{
    public static MobilityAuditView ToMobilityAuditView(MobilityAudit item)
    {
        return new MobilityAuditView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 40),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 40),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityAuditView> ToMobilityAuditViews(List<MobilityAudit> items)
    {
        var views = new List<MobilityAuditView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityAuditView(item));
        }
        return views;
    }

    public static double SumMobilityAuditView(MobilityAuditView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return parsedWeight + parsedMinimum + (double)parsedTenureMonths;
    }
}
