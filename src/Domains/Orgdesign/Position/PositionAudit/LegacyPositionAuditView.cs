namespace Acme.Workforce.Orgdesign.Position.PositionAudit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy position audit.</summary>
public sealed record LegacyPositionAuditView(string Code, string TitleText, string LeaveDays, string Team, string ExternalRef, string Badge, string StatusLabel)
{
    public static LegacyPositionAuditView ToLegacyPositionAuditView(LegacyPositionAudit item)
    {
        return new LegacyPositionAuditView(
            item.Code,
            item.Title,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.ExternalRef,
            Text.Truncate(item.Badge, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyPositionAuditView> ToLegacyPositionAuditViews(List<LegacyPositionAudit> items)
    {
        var views = new List<LegacyPositionAuditView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyPositionAuditView(item));
        }
        return views;
    }

    public static double SumLegacyPositionAuditView(LegacyPositionAuditView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays;
    }
}
