namespace Acme.Workforce.Absence.Balance.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled balance agreement.</summary>
public sealed record ScheduledBalanceAgreementView(string Code, string Title, string BatchSize, string SourceText, string Budget, string Maximum, string Approved, string GraceDays, string CeilingText, string WindowDays, string Currency, string StatusLabel)
{
    public static ScheduledBalanceAgreementView ToScheduledBalanceAgreementView(ScheduledBalanceAgreement item)
    {
        return new ScheduledBalanceAgreementView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledBalanceAgreementView> ToScheduledBalanceAgreementViews(List<ScheduledBalanceAgreement> items)
    {
        var views = new List<ScheduledBalanceAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledBalanceAgreementView(item));
        }
        return views;
    }

    public static double SumScheduledBalanceAgreementView(ScheduledBalanceAgreementView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedBudget + parsedMaximum;
    }
}
