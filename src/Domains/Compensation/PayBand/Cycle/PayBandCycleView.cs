namespace Acme.Workforce.Compensation.PayBand.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay band cycle.</summary>
public sealed record PayBandCycleView(string CodeText, string NameText, string Quantity, string Hours, string ManagerIdText, string FloorValue, string Skill, string Cost, string Discount, string StatusLabel)
{
    public static PayBandCycleView ToPayBandCycleView(PayBandCycle item)
    {
        return new PayBandCycleView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayBandCycleView> ToPayBandCycleViews(List<PayBandCycle> items)
    {
        var views = new List<PayBandCycleView>();
        foreach (var item in items)
        {
            views.Add(ToPayBandCycleView(item));
        }
        return views;
    }

    public static double SumPayBandCycleView(PayBandCycleView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedHours + parsedFloorValue;
    }
}
