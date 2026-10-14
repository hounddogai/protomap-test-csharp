namespace Acme.Workforce.Assets.Laptop.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient laptop window.</summary>
public sealed record LenientLaptopWindowView(string Code, string NameText, string Phone, string LeaveDays, string Department, string Region, string Score, string BatchSize, string ContactEmailText, string StatusLabel)
{
    public static LenientLaptopWindowView ToLenientLaptopWindowView(LenientLaptopWindow item)
    {
        return new LenientLaptopWindowView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Phone,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Region,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientLaptopWindowView> ToLenientLaptopWindowViews(List<LenientLaptopWindow> items)
    {
        var views = new List<LenientLaptopWindowView>();
        foreach (var item in items)
        {
            views.Add(ToLenientLaptopWindowView(item));
        }
        return views;
    }

    public static double SumLenientLaptopWindowView(LenientLaptopWindowView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedScore + (double)parsedBatchSize;
    }
}
