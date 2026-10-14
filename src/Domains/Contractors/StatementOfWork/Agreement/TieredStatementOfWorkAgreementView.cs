namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered statement of work agreement.</summary>
public sealed record TieredStatementOfWorkAgreementView(string CodeText, string NameText, string EndsAtText, string GraceDaysText, string UpdatedAt, string ManagerId, string StatusLabel)
{
    public static TieredStatementOfWorkAgreementView ToTieredStatementOfWorkAgreementView(TieredStatementOfWorkAgreement item)
    {
        return new TieredStatementOfWorkAgreementView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredStatementOfWorkAgreementView> ToTieredStatementOfWorkAgreementViews(List<TieredStatementOfWorkAgreement> items)
    {
        var views = new List<TieredStatementOfWorkAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToTieredStatementOfWorkAgreementView(item));
        }
        return views;
    }

    public static double SumTieredStatementOfWorkAgreementView(TieredStatementOfWorkAgreementView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedGraceDays + (double)parsedUpdatedAt;
    }
}
