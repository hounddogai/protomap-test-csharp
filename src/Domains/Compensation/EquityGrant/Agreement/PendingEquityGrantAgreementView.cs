namespace Acme.Workforce.Compensation.EquityGrant.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending equity grant agreement.</summary>
public sealed record PendingEquityGrantAgreementView(string CodeText, string Label, string Notes, string LocationText, string Maximum, string Locale, string ManagerId, string Name, string EmployeeIdText, string StatusLabel)
{
    public static PendingEquityGrantAgreementView ToPendingEquityGrantAgreementView(PendingEquityGrantAgreement item)
    {
        return new PendingEquityGrantAgreementView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Notes,
            item.Location,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Locale, 80),
            item.ManagerId,
            item.Name,
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingEquityGrantAgreementView> ToPendingEquityGrantAgreementViews(List<PendingEquityGrantAgreement> items)
    {
        var views = new List<PendingEquityGrantAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToPendingEquityGrantAgreementView(item));
        }
        return views;
    }

    public static double SumPendingEquityGrantAgreementView(PendingEquityGrantAgreementView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedMaximum;
    }
}
