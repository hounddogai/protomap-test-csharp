namespace Acme.Workforce.Compliance.Attestation.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a attestation schedule.</summary>
public sealed record AttestationScheduleView(string CodeText, string Name, string Archived, string CurrencyText, string Unit, string JobCode, string ExternalRefText, string RateText, string AttemptsText, string Badge, string Channel, string StatusLabel)
{
    public static AttestationScheduleView ToAttestationScheduleView(AttestationSchedule item)
    {
        return new AttestationScheduleView(
            Text.Truncate(item.Code, 80),
            item.Name,
            $"{item.Archived}",
            Text.Truncate(item.Currency, 80),
            item.Unit,
            item.JobCode,
            item.ExternalRef,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 64),
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationScheduleView> ToAttestationScheduleViews(List<AttestationSchedule> items)
    {
        var views = new List<AttestationScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationScheduleView(item));
        }
        return views;
    }

    public static double SumAttestationScheduleView(AttestationScheduleView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedAttempts;
    }
}
