namespace Acme.Workforce.Talent.CareerPath.PathSummary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending career path batch.</summary>
public sealed record PendingCareerPathBatchView(string CodeText, string TitleText, string CreatedAt, string EmployeeId, string BatchSize, string Hours, string ContactEmailText, string StatusLabel)
{
    public static PendingCareerPathBatchView ToPendingCareerPathBatchView(PendingCareerPathBatch item)
    {
        return new PendingCareerPathBatchView(
            item.Code,
            item.Title,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 120),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingCareerPathBatchView> ToPendingCareerPathBatchViews(List<PendingCareerPathBatch> items)
    {
        var views = new List<PendingCareerPathBatchView>();
        foreach (var item in items)
        {
            views.Add(ToPendingCareerPathBatchView(item));
        }
        return views;
    }

    public static double SumPendingCareerPathBatchView(PendingCareerPathBatchView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedBatchSize + parsedHours;
    }
}
