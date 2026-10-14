namespace Acme.Workforce.Payrollops.Garnishment.Exception;

using System.Globalization;

/// <summary>How screens and exports show a batched garnishment exception.</summary>
public sealed record BatchedGarnishmentExceptionView(string Code, string Title, string Revision, string EmployeeId, string SkillText, string EndsAt, string OwnerId, string StatusLabel)
{
    public static BatchedGarnishmentExceptionView ToBatchedGarnishmentExceptionView(BatchedGarnishmentException item)
    {
        return new BatchedGarnishmentExceptionView(
            item.Code,
            item.Title,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Skill,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedGarnishmentExceptionView> ToBatchedGarnishmentExceptionViews(List<BatchedGarnishmentException> items)
    {
        var views = new List<BatchedGarnishmentExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedGarnishmentExceptionView(item));
        }
        return views;
    }

    public static double SumBatchedGarnishmentExceptionView(BatchedGarnishmentExceptionView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedEndsAt;
    }
}
