namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending skill profile digest.</summary>
public sealed record PendingSkillProfileDigestView(string CodeText, string NameText, string Threshold, string EmployeeIdText, string PriorityText, string SequenceText, string StatusLabel)
{
    public static PendingSkillProfileDigestView ToPendingSkillProfileDigestView(PendingSkillProfileDigest item)
    {
        return new PendingSkillProfileDigestView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 80),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingSkillProfileDigestView> ToPendingSkillProfileDigestViews(List<PendingSkillProfileDigest> items)
    {
        var views = new List<PendingSkillProfileDigestView>();
        foreach (var item in items)
        {
            views.Add(ToPendingSkillProfileDigestView(item));
        }
        return views;
    }

    public static double SumPendingSkillProfileDigestView(PendingSkillProfileDigestView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedPriority + (double)parsedSequence;
    }
}
