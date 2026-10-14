namespace Acme.Workforce.Payrollops.PayRun.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay run assignment.</summary>
public sealed record PayRunAssignmentView(string Code, string Label, string Rating, string AttemptsText, string Summary, string LocationText, string ReviewerText, string StatusLabel)
{
    public static PayRunAssignmentView ToPayRunAssignmentView(PayRunAssignment item)
    {
        return new PayRunAssignmentView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.Location,
            Text.Truncate(item.Reviewer, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunAssignmentView> ToPayRunAssignmentViews(List<PayRunAssignment> items)
    {
        var views = new List<PayRunAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunAssignmentView(item));
        }
        return views;
    }

    public static double SumPayRunAssignmentView(PayRunAssignmentView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedAttempts;
    }
}
