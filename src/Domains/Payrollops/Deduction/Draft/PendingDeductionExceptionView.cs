namespace Acme.Workforce.Payrollops.Deduction.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending deduction exception.</summary>
public sealed record PendingDeductionExceptionView(string Code, string Label, string CountryText, string Sequence, string EndsAt, string ExternalRef, string BatchSize, string LeaveDaysText, string Channel, string ContactEmail, string Minimum, string StatusLabel)
{
    public static PendingDeductionExceptionView ToPendingDeductionExceptionView(PendingDeductionException item)
    {
        return new PendingDeductionExceptionView(
            item.Code,
            Text.Truncate(item.Label, 80),
            Text.Truncate(item.Country, 120),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 64),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            Text.Truncate(item.ContactEmail, 80),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingDeductionExceptionView> ToPendingDeductionExceptionViews(List<PendingDeductionException> items)
    {
        var views = new List<PendingDeductionExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToPendingDeductionExceptionView(item));
        }
        return views;
    }

    public static double SumPendingDeductionExceptionView(PendingDeductionExceptionView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedEndsAt + (double)parsedBatchSize;
    }
}
