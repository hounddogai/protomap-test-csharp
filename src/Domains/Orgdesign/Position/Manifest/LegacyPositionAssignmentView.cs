namespace Acme.Workforce.Orgdesign.Position.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy position assignment.</summary>
public sealed record LegacyPositionAssignmentView(string Code, string Title, string Retries, string JobCodeText, string ExpiresAtText, string SkillText, string LocationText, string Maximum, string Label, string Archived, string StatusLabel)
{
    public static LegacyPositionAssignmentView ToLegacyPositionAssignmentView(LegacyPositionAssignment item)
    {
        return new LegacyPositionAssignmentView(
            item.Code,
            item.Title,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.Location,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Label, 120),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyPositionAssignmentView> ToLegacyPositionAssignmentViews(List<LegacyPositionAssignment> items)
    {
        var views = new List<LegacyPositionAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyPositionAssignmentView(item));
        }
        return views;
    }

    public static double SumLegacyPositionAssignmentView(LegacyPositionAssignmentView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedExpiresAt + parsedMaximum;
    }
}
