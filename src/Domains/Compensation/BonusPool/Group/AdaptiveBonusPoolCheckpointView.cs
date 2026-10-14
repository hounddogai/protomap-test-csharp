namespace Acme.Workforce.Compensation.BonusPool.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive bonus pool checkpoint.</summary>
public sealed record AdaptiveBonusPoolCheckpointView(string CodeText, string NameText, string CreatedAt, string Location, string ChecksumText, string ReviewerText, string JobCode, string Source, string Notes, string StatusLabel)
{
    public static AdaptiveBonusPoolCheckpointView ToAdaptiveBonusPoolCheckpointView(AdaptiveBonusPoolCheckpoint item)
    {
        return new AdaptiveBonusPoolCheckpointView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.Checksum,
            item.Reviewer,
            item.JobCode,
            item.Source,
            Text.Truncate(item.Notes, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveBonusPoolCheckpointView> ToAdaptiveBonusPoolCheckpointViews(List<AdaptiveBonusPoolCheckpoint> items)
    {
        var views = new List<AdaptiveBonusPoolCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveBonusPoolCheckpointView(item));
        }
        return views;
    }

    public static double SumAdaptiveBonusPoolCheckpointView(AdaptiveBonusPoolCheckpointView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt;
    }
}
