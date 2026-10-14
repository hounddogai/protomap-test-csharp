namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using System.Globalization;

/// <summary>How screens and exports show a legacy statement of work agreement.</summary>
public sealed record LegacyStatementOfWorkAgreementView(string Code, string Title, string Country, string Category, string Amount, string TenureMonths, string BudgetText, string StatusLabel)
{
    public static LegacyStatementOfWorkAgreementView ToLegacyStatementOfWorkAgreementView(LegacyStatementOfWorkAgreement item)
    {
        return new LegacyStatementOfWorkAgreementView(
            item.Code,
            item.Title,
            item.Country,
            item.Category,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyStatementOfWorkAgreementView> ToLegacyStatementOfWorkAgreementViews(List<LegacyStatementOfWorkAgreement> items)
    {
        var views = new List<LegacyStatementOfWorkAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyStatementOfWorkAgreementView(item));
        }
        return views;
    }

    public static double SumLegacyStatementOfWorkAgreementView(LegacyStatementOfWorkAgreementView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedTenureMonths + parsedBudget;
    }
}
