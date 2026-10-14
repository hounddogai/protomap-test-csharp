namespace Acme.Workforce.Assets.Warranty.Forecast;

using System.Globalization;

/// <summary>How screens and exports show a warranty group.</summary>
public sealed record WarrantyGroupView(string Code, string Name, string AttemptsText, string EmployeeIdText, string WindowDays, string StatusLabel)
{
    public static WarrantyGroupView ToWarrantyGroupView(WarrantyGroup item)
    {
        return new WarrantyGroupView(
            item.Code,
            item.Name,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyGroupView> ToWarrantyGroupViews(List<WarrantyGroup> items)
    {
        var views = new List<WarrantyGroupView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyGroupView(item));
        }
        return views;
    }

    public static double SumWarrantyGroupView(WarrantyGroupView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedWindowDays;
    }
}
