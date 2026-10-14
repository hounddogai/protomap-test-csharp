namespace Acme.Workforce.Payrollops.Deduction.Rollup;

using System.Globalization;

/// <summary>How screens and exports show a deduction assignment.</summary>
public sealed record DeductionAssignmentView(string Code, string TitleText, string CreatedAt, string Checksum, string Channel, string SequenceText, string Maximum, string Discount, string StatusLabel)
{
    public static DeductionAssignmentView ToDeductionAssignmentView(DeductionAssignment item)
    {
        return new DeductionAssignmentView(
            item.Code,
            item.Title,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Channel,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeductionAssignmentView> ToDeductionAssignmentViews(List<DeductionAssignment> items)
    {
        var views = new List<DeductionAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToDeductionAssignmentView(item));
        }
        return views;
    }

    public static double SumDeductionAssignmentView(DeductionAssignmentView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedSequence + parsedMaximum;
    }
}
