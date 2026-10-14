namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default training due audit.</summary>
public sealed record DefaultTrainingDueAuditView(string CodeText, string Title, string Description, string Unit, string Country, string SourceText, string ArchivedText, string EndsAtText, string VisibleText, string UpdatedAtText, string Department, string StatusLabel)
{
    public static DefaultTrainingDueAuditView ToDefaultTrainingDueAuditView(DefaultTrainingDueAudit item)
    {
        return new DefaultTrainingDueAuditView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 80),
            Text.Truncate(item.Description, 64),
            item.Unit,
            item.Country,
            item.Source,
            $"{item.Archived}",
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultTrainingDueAuditView> ToDefaultTrainingDueAuditViews(List<DefaultTrainingDueAudit> items)
    {
        var views = new List<DefaultTrainingDueAuditView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultTrainingDueAuditView(item));
        }
        return views;
    }

    public static double SumDefaultTrainingDueAuditView(DefaultTrainingDueAuditView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedUpdatedAt;
    }
}
