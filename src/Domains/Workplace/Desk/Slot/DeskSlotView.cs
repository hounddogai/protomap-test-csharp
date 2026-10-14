namespace Acme.Workforce.Workplace.Desk.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a desk slot.</summary>
public sealed record DeskSlotView(string Code, string LabelText, string Pinned, string CountryText, string ContactEmail, string Channel, string UpdatedAt, string Region, string ManagerIdText, string RateText, string Active, string StatusLabel)
{
    public static DeskSlotView ToDeskSlotView(DeskSlot item)
    {
        return new DeskSlotView(
            Text.Truncate(item.Code, 40),
            item.Label,
            $"{item.Pinned}",
            item.Country,
            Text.Truncate(item.ContactEmail, 120),
            Text.Truncate(item.Channel, 80),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Region,
            item.ManagerId,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskSlotView> ToDeskSlotViews(List<DeskSlot> items)
    {
        var views = new List<DeskSlotView>();
        foreach (var item in items)
        {
            views.Add(ToDeskSlotView(item));
        }
        return views;
    }

    public static double SumDeskSlotView(DeskSlotView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedRate;
    }
}
