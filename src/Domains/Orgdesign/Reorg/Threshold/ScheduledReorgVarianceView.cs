namespace Acme.Workforce.Orgdesign.Reorg.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled reorg variance.</summary>
public sealed record ScheduledReorgVarianceView(string CodeText, string LabelText, string Retries, string Timezone, string ContactEmail, string Visible, string OwnerId, string SkillText, string Checksum, string StatusLabel)
{
    public static ScheduledReorgVarianceView ToScheduledReorgVarianceView(ScheduledReorgVariance item)
    {
        return new ScheduledReorgVarianceView(
            item.Code,
            item.Label,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 80),
            item.ContactEmail,
            $"{item.Visible}",
            Text.Truncate(item.OwnerId, 80),
            Text.Truncate(item.Skill, 80),
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledReorgVarianceView> ToScheduledReorgVarianceViews(List<ScheduledReorgVariance> items)
    {
        var views = new List<ScheduledReorgVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledReorgVarianceView(item));
        }
        return views;
    }

    public static double SumScheduledReorgVarianceView(ScheduledReorgVarianceView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedRetries;
    }
}
