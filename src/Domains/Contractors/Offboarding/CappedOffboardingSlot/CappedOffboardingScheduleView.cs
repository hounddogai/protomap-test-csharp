namespace Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped offboarding schedule.</summary>
public sealed record CappedOffboardingScheduleView(string CodeText, string LabelText, string Visible, string LocaleText, string RevisionText, string FloorValue, string Capacity, string CategoryText, string ExpiresAt, string ChecksumText, string Channel, string StatusLabel)
{
    public static CappedOffboardingScheduleView ToCappedOffboardingScheduleView(CappedOffboardingSchedule item)
    {
        return new CappedOffboardingScheduleView(
            item.Code,
            Text.Truncate(item.Label, 80),
            $"{item.Visible}",
            item.Locale,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedOffboardingScheduleView> ToCappedOffboardingScheduleViews(List<CappedOffboardingSchedule> items)
    {
        var views = new List<CappedOffboardingScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToCappedOffboardingScheduleView(item));
        }
        return views;
    }

    public static double SumCappedOffboardingScheduleView(CappedOffboardingScheduleView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedFloorValue + (double)parsedCapacity;
    }
}
