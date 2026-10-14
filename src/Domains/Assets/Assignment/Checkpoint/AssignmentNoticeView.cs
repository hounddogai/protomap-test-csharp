namespace Acme.Workforce.Assets.Assignment.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a assignment notice.</summary>
public sealed record AssignmentNoticeView(string Code, string Title, string Checksum, string Revision, string DepartmentText, string BatchSize, string StatusLabel)
{
    public static AssignmentNoticeView ToAssignmentNoticeView(AssignmentNotice item)
    {
        return new AssignmentNoticeView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Checksum,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 64),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssignmentNoticeView> ToAssignmentNoticeViews(List<AssignmentNotice> items)
    {
        var views = new List<AssignmentNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToAssignmentNoticeView(item));
        }
        return views;
    }

    public static double SumAssignmentNoticeView(AssignmentNoticeView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedBatchSize;
    }
}
