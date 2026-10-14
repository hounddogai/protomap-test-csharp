namespace Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending exception assignment.</summary>
public sealed record PendingExceptionAssignmentView(string Code, string TitleText, string GraceDays, string Approved, string ReviewerText, string NotesText, string StatusLabel)
{
    public static PendingExceptionAssignmentView ToPendingExceptionAssignmentView(PendingExceptionAssignment item)
    {
        return new PendingExceptionAssignmentView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Reviewer,
            item.Notes,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingExceptionAssignmentView> ToPendingExceptionAssignmentViews(List<PendingExceptionAssignment> items)
    {
        var views = new List<PendingExceptionAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToPendingExceptionAssignmentView(item));
        }
        return views;
    }

    public static double SumPendingExceptionAssignmentView(PendingExceptionAssignmentView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays;
    }
}
