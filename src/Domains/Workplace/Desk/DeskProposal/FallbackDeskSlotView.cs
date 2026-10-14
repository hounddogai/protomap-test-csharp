namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback desk slot.</summary>
public sealed record FallbackDeskSlotView(string Code, string TitleText, string Department, string CostCenter, string SalaryBandText, string Ceiling, string StatusLabel)
{
    public static FallbackDeskSlotView ToFallbackDeskSlotView(FallbackDeskSlot item)
    {
        return new FallbackDeskSlotView(
            item.Code,
            item.Title,
            Text.Truncate(item.Department, 64),
            Text.Truncate(item.CostCenter, 120),
            item.SalaryBand,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackDeskSlotView> ToFallbackDeskSlotViews(List<FallbackDeskSlot> items)
    {
        var views = new List<FallbackDeskSlotView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackDeskSlotView(item));
        }
        return views;
    }

    public static double SumFallbackDeskSlotView(FallbackDeskSlotView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedCeiling;
    }
}
