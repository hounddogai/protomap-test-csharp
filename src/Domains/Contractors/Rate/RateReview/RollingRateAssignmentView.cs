namespace Acme.Workforce.Contractors.Rate.RateReview;

using System.Globalization;

/// <summary>How screens and exports show a rolling rate assignment.</summary>
public sealed record RollingRateAssignmentView(string CodeText, string LabelText, string SourceText, string Sequence, string Rate, string Approved, string StatusLabel)
{
    public static RollingRateAssignmentView ToRollingRateAssignmentView(RollingRateAssignment item)
    {
        return new RollingRateAssignmentView(
            item.Code,
            item.Label,
            item.Source,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingRateAssignmentView> ToRollingRateAssignmentViews(List<RollingRateAssignment> items)
    {
        var views = new List<RollingRateAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToRollingRateAssignmentView(item));
        }
        return views;
    }

    public static double SumRollingRateAssignmentView(RollingRateAssignmentView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedRate;
    }
}
