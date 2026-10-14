namespace Acme.Workforce.Payrollops.Deduction.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient deduction draft.</summary>
public sealed record LenientDeductionDraftView(string Code, string NameText, string FloorValue, string RevisionText, string Category, string Description, string Ceiling, string PriorityText, string GraceDays, string Reviewer, string StatusLabel)
{
    public static LenientDeductionDraftView ToLenientDeductionDraftView(LenientDeductionDraft item)
    {
        return new LenientDeductionDraftView(
            item.Code,
            item.Name,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Category,
            Text.Truncate(item.Description, 64),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientDeductionDraftView> ToLenientDeductionDraftViews(List<LenientDeductionDraft> items)
    {
        var views = new List<LenientDeductionDraftView>();
        foreach (var item in items)
        {
            views.Add(ToLenientDeductionDraftView(item));
        }
        return views;
    }

    public static double SumLenientDeductionDraftView(LenientDeductionDraftView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedRevision + parsedCeiling;
    }
}
