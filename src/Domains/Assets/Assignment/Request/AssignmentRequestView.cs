namespace Acme.Workforce.Assets.Assignment.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a assignment request.</summary>
public sealed record AssignmentRequestView(string Code, string Label, string ScoreText, string ArchivedText, string Timezone, string StatusLabel)
{
    public static AssignmentRequestView ToAssignmentRequestView(AssignmentRequest item)
    {
        return new AssignmentRequestView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            Text.Truncate(item.Timezone, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssignmentRequestView> ToAssignmentRequestViews(List<AssignmentRequest> items)
    {
        var views = new List<AssignmentRequestView>();
        foreach (var item in items)
        {
            views.Add(ToAssignmentRequestView(item));
        }
        return views;
    }

    public static double SumAssignmentRequestView(AssignmentRequestView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
