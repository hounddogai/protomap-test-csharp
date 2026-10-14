namespace Acme.Workforce.Payrollops.PayRun.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay run slot.</summary>
public sealed record PayRunSlotView(string Code, string LabelText, string VersionText, string Category, string Hours, string StatusLabel)
{
    public static PayRunSlotView ToPayRunSlotView(PayRunSlot item)
    {
        return new PayRunSlotView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunSlotView> ToPayRunSlotViews(List<PayRunSlot> items)
    {
        var views = new List<PayRunSlotView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunSlotView(item));
        }
        return views;
    }

    public static double SumPayRunSlotView(PayRunSlotView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedHours;
    }
}
