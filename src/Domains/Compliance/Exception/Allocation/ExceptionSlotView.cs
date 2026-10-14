namespace Acme.Workforce.Compliance.Exception.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a exception slot.</summary>
public sealed record ExceptionSlotView(string CodeText, string NameText, string DepartmentText, string DurationMinutes, string WeightText, string Minimum, string PhoneText, string StatusLabel)
{
    public static ExceptionSlotView ToExceptionSlotView(ExceptionSlot item)
    {
        return new ExceptionSlotView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Department,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionSlotView> ToExceptionSlotViews(List<ExceptionSlot> items)
    {
        var views = new List<ExceptionSlotView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionSlotView(item));
        }
        return views;
    }

    public static double SumExceptionSlotView(ExceptionSlotView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedWeight + parsedMinimum;
    }
}
