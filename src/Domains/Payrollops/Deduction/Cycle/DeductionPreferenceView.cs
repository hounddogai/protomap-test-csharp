namespace Acme.Workforce.Payrollops.Deduction.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a deduction preference.</summary>
public sealed record DeductionPreferenceView(string Code, string Name, string Phone, string FteText, string Maximum, string FloorValue, string Grade, string StatusLabel)
{
    public static DeductionPreferenceView ToDeductionPreferenceView(DeductionPreference item)
    {
        return new DeductionPreferenceView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Phone,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeductionPreferenceView> ToDeductionPreferenceViews(List<DeductionPreference> items)
    {
        var views = new List<DeductionPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToDeductionPreferenceView(item));
        }
        return views;
    }

    public static double SumDeductionPreferenceView(DeductionPreferenceView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedFte + parsedMaximum + parsedFloorValue;
    }
}
