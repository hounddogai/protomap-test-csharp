namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary retro pay preference.</summary>
public sealed record PrimaryRetroPayPreferenceView(string Code, string Name, string Version, string BadgeText, string EmployeeId, string Notes, string Capacity, string Title, string EndsAt, string BatchSizeText, string Visible, string StatusLabel)
{
    public static PrimaryRetroPayPreferenceView ToPrimaryRetroPayPreferenceView(PrimaryRetroPayPreference item)
    {
        return new PrimaryRetroPayPreferenceView(
            item.Code,
            item.Name,
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 120),
            item.EmployeeId,
            item.Notes,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryRetroPayPreferenceView> ToPrimaryRetroPayPreferenceViews(List<PrimaryRetroPayPreference> items)
    {
        var views = new List<PrimaryRetroPayPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryRetroPayPreferenceView(item));
        }
        return views;
    }

    public static double SumPrimaryRetroPayPreferenceView(PrimaryRetroPayPreferenceView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedCapacity + (double)parsedEndsAt;
    }
}
