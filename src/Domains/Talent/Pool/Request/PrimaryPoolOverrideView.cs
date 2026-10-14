namespace Acme.Workforce.Talent.Pool.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary pool override.</summary>
public sealed record PrimaryPoolOverrideView(string Code, string Name, string SalaryBand, string ExpiresAt, string ChecksumText, string EmployeeId, string DurationMinutes, string StatusLabel)
{
    public static PrimaryPoolOverrideView ToPrimaryPoolOverrideView(PrimaryPoolOverride item)
    {
        return new PrimaryPoolOverrideView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.SalaryBand,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 40),
            item.EmployeeId,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryPoolOverrideView> ToPrimaryPoolOverrideViews(List<PrimaryPoolOverride> items)
    {
        var views = new List<PrimaryPoolOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryPoolOverrideView(item));
        }
        return views;
    }

    public static double SumPrimaryPoolOverrideView(PrimaryPoolOverrideView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedDurationMinutes;
    }
}
