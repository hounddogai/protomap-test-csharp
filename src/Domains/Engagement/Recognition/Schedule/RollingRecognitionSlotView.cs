namespace Acme.Workforce.Engagement.Recognition.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling recognition slot.</summary>
public sealed record RollingRecognitionSlotView(string Code, string Label, string Phone, string Quantity, string CurrencyText, string Description, string Pinned, string LeaveDaysText, string VersionText, string Country, string StatusLabel)
{
    public static RollingRecognitionSlotView ToRollingRecognitionSlotView(RollingRecognitionSlot item)
    {
        return new RollingRecognitionSlotView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Phone,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            Text.Truncate(item.Description, 64),
            $"{item.Pinned}",
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingRecognitionSlotView> ToRollingRecognitionSlotViews(List<RollingRecognitionSlot> items)
    {
        var views = new List<RollingRecognitionSlotView>();
        foreach (var item in items)
        {
            views.Add(ToRollingRecognitionSlotView(item));
        }
        return views;
    }

    public static double SumRollingRecognitionSlotView(RollingRecognitionSlotView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedLeaveDays + (double)parsedVersion;
    }
}
