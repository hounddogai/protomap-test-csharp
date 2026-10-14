namespace Acme.Workforce.Assets.Laptop.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a laptop window.</summary>
public sealed record LaptopWindowView(string CodeText, string Name, string ApprovedText, string ContactEmail, string TenureMonths, string MarginText, string Grade, string Rating, string BadgeText, string EmployeeIdText, string StatusLabel)
{
    public static LaptopWindowView ToLaptopWindowView(LaptopWindow item)
    {
        return new LaptopWindowView(
            item.Code,
            item.Name,
            $"{item.Approved}",
            Text.Truncate(item.ContactEmail, 80),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopWindowView> ToLaptopWindowViews(List<LaptopWindow> items)
    {
        var views = new List<LaptopWindowView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopWindowView(item));
        }
        return views;
    }

    public static double SumLaptopWindowView(LaptopWindowView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedMargin + parsedRating;
    }
}
