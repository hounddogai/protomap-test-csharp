namespace Acme.Workforce.Compliance.Attestation.AttestationDraft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive attestation draft.</summary>
public sealed record AdaptiveAttestationDraftView(string Code, string Name, string BatchSize, string EmployeeId, string JobCodeText, string PhoneText, string TimezoneText, string Active, string LeaveDays, string SalaryBandText, string Currency, string StatusLabel)
{
    public static AdaptiveAttestationDraftView ToAdaptiveAttestationDraftView(AdaptiveAttestationDraft item)
    {
        return new AdaptiveAttestationDraftView(
            item.Code,
            item.Name,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.JobCode,
            Text.Truncate(item.Phone, 80),
            item.Timezone,
            $"{item.Active}",
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveAttestationDraftView> ToAdaptiveAttestationDraftViews(List<AdaptiveAttestationDraft> items)
    {
        var views = new List<AdaptiveAttestationDraftView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveAttestationDraftView(item));
        }
        return views;
    }

    public static double SumAdaptiveAttestationDraftView(AdaptiveAttestationDraftView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedLeaveDays;
    }
}
