namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional attestation schedule.</summary>
public sealed record RegionalAttestationScheduleView(string CodeText, string Name, string MaximumText, string ManagerId, string TitleText, string ActiveText, string Department, string StatusLabel)
{
    public static RegionalAttestationScheduleView ToRegionalAttestationScheduleView(RegionalAttestationSchedule item)
    {
        return new RegionalAttestationScheduleView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 40),
            Text.Truncate(item.Title, 120),
            $"{item.Active}",
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalAttestationScheduleView> ToRegionalAttestationScheduleViews(List<RegionalAttestationSchedule> items)
    {
        var views = new List<RegionalAttestationScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalAttestationScheduleView(item));
        }
        return views;
    }

    public static double SumRegionalAttestationScheduleView(RegionalAttestationScheduleView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return parsedMaximum;
    }
}
