namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work agreement.</summary>
public sealed record StatementOfWorkAgreementView(string Code, string Title, string GraceDays, string Discount, string Timezone, string Region, string StatusLabel)
{
    public static StatementOfWorkAgreementView ToStatementOfWorkAgreementView(StatementOfWorkAgreement item)
    {
        return new StatementOfWorkAgreementView(
            item.Code,
            item.Title,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 80),
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkAgreementView> ToStatementOfWorkAgreementViews(List<StatementOfWorkAgreement> items)
    {
        var views = new List<StatementOfWorkAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkAgreementView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkAgreementView(StatementOfWorkAgreementView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedDiscount;
    }
}
