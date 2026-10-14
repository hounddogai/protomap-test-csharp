namespace Acme.Workforce.Compensation.PayBand.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy pay band agreement.</summary>
public sealed record LegacyPayBandAgreementView(string CodeText, string LabelText, string SalaryBandText, string FteText, string TimezoneText, string Country, string EmployeeId, string Badge, string Notes, string CostCenter, string StatusLabel)
{
    public static LegacyPayBandAgreementView ToLegacyPayBandAgreementView(LegacyPayBandAgreement item)
    {
        return new LegacyPayBandAgreementView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.SalaryBand,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.Country,
            item.EmployeeId,
            item.Badge,
            item.Notes,
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyPayBandAgreementView> ToLegacyPayBandAgreementViews(List<LegacyPayBandAgreement> items)
    {
        var views = new List<LegacyPayBandAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyPayBandAgreementView(item));
        }
        return views;
    }

    public static double SumLegacyPayBandAgreementView(LegacyPayBandAgreementView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return parsedFte;
    }
}
