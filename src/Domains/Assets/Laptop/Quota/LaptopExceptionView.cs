namespace Acme.Workforce.Assets.Laptop.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a laptop exception.</summary>
public sealed record LaptopExceptionView(string Code, string Label, string Archived, string EndsAt, string ReviewerText, string TitleText, string EmployeeIdText, string TenureMonths, string BatchSize, string Notes, string StatusLabel)
{
    public static LaptopExceptionView ToLaptopExceptionView(LaptopException item)
    {
        return new LaptopExceptionView(
            Text.Truncate(item.Code, 80),
            item.Label,
            $"{item.Archived}",
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Title,
            Text.Truncate(item.EmployeeId, 120),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopExceptionView> ToLaptopExceptionViews(List<LaptopException> items)
    {
        var views = new List<LaptopExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopExceptionView(item));
        }
        return views;
    }

    public static double SumLaptopExceptionView(LaptopExceptionView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedTenureMonths + (double)parsedBatchSize;
    }
}
