namespace Acme.Workforce.Talent.SuccessionPlan.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy succession plan policy.</summary>
public sealed record LegacySuccessionPlanPolicyView(string CodeText, string LabelText, string Country, string SegmentText, string Checksum, string Sequence, string CreatedAt, string Priority, string GraceDaysText, string Approved, string SalaryBand, string StatusLabel)
{
    public static LegacySuccessionPlanPolicyView ToLegacySuccessionPlanPolicyView(LegacySuccessionPlanPolicy item)
    {
        return new LegacySuccessionPlanPolicyView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Country,
            Text.Truncate(item.Segment, 40),
            item.Checksum,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.SalaryBand,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacySuccessionPlanPolicyView> ToLegacySuccessionPlanPolicyViews(List<LegacySuccessionPlanPolicy> items)
    {
        var views = new List<LegacySuccessionPlanPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToLegacySuccessionPlanPolicyView(item));
        }
        return views;
    }

    public static double SumLegacySuccessionPlanPolicyView(LegacySuccessionPlanPolicyView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedCreatedAt + (double)parsedPriority;
    }
}
