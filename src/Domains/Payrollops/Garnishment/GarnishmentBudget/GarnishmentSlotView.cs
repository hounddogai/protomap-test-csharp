namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a garnishment slot.</summary>
public sealed record GarnishmentSlotView(string CodeText, string Title, string Name, string Maximum, string ManagerId, string JobCodeText, string Notes, string Approved, string CeilingText, string GraceDaysText, string StatusLabel)
{
    public static GarnishmentSlotView ToGarnishmentSlotView(GarnishmentSlot item)
    {
        return new GarnishmentSlotView(
            Text.Truncate(item.Code, 120),
            item.Title,
            Text.Truncate(item.Name, 80),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            Text.Truncate(item.JobCode, 40),
            item.Notes,
            $"{item.Approved}",
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GarnishmentSlotView> ToGarnishmentSlotViews(List<GarnishmentSlot> items)
    {
        var views = new List<GarnishmentSlotView>();
        foreach (var item in items)
        {
            views.Add(ToGarnishmentSlotView(item));
        }
        return views;
    }

    public static double SumGarnishmentSlotView(GarnishmentSlotView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedCeiling + (double)parsedGraceDays;
    }
}
