namespace Acme.Workforce.Compliance.Audit.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default audit checkpoint.</summary>
public sealed record DefaultAuditCheckpointView(string Code, string NameText, string EmployeeIdText, string ContactEmail, string StartsAtText, string CategoryText, string ManagerIdText, string TimezoneText, string Label, string Active, string Source, string StatusLabel)
{
    public static DefaultAuditCheckpointView ToDefaultAuditCheckpointView(DefaultAuditCheckpoint item)
    {
        return new DefaultAuditCheckpointView(
            item.Code,
            item.Name,
            item.EmployeeId,
            item.ContactEmail,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 64),
            item.ManagerId,
            Text.Truncate(item.Timezone, 64),
            item.Label,
            $"{item.Active}",
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultAuditCheckpointView> ToDefaultAuditCheckpointViews(List<DefaultAuditCheckpoint> items)
    {
        var views = new List<DefaultAuditCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultAuditCheckpointView(item));
        }
        return views;
    }

    public static double SumDefaultAuditCheckpointView(DefaultAuditCheckpointView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt;
    }
}
