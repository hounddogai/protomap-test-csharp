namespace Acme.Workforce.Absence.Holiday.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a holiday policy.</summary>
public sealed record HolidayPolicyView(string Code, string NameText, string Location, string Fte, string Channel, string StatusLabel)
{
    public static HolidayPolicyView ToHolidayPolicyView(HolidayPolicy item)
    {
        return new HolidayPolicyView(
            item.Code,
            item.Name,
            item.Location,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HolidayPolicyView> ToHolidayPolicyViews(List<HolidayPolicy> items)
    {
        var views = new List<HolidayPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToHolidayPolicyView(item));
        }
        return views;
    }

    public static double SumHolidayPolicyView(HolidayPolicyView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedFte;
    }
}
