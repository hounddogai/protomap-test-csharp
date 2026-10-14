namespace Acme.Workforce.Contractors.Offboarding.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary offboarding batch.</summary>
public sealed record PrimaryOffboardingBatchView(string Code, string Name, string Limit, string Reviewer, string Priority, string Sequence, string Fte, string LeaveDaysText, string StatusLabel)
{
    public static PrimaryOffboardingBatchView ToPrimaryOffboardingBatchView(PrimaryOffboardingBatch item)
    {
        return new PrimaryOffboardingBatchView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryOffboardingBatchView> ToPrimaryOffboardingBatchViews(List<PrimaryOffboardingBatch> items)
    {
        var views = new List<PrimaryOffboardingBatchView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryOffboardingBatchView(item));
        }
        return views;
    }

    public static double SumPrimaryOffboardingBatchView(PrimaryOffboardingBatchView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedPriority + (double)parsedSequence;
    }
}
