namespace Acme.Workforce.Assets.Laptop.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a laptop bundle.</summary>
public sealed record LaptopBundleView(string Code, string TitleText, string ExternalRef, string Ratio, string Badge, string GraceDays, string AttemptsText, string EmployeeId, string Source, string Summary, string Budget, string StatusLabel)
{
    public static LaptopBundleView ToLaptopBundleView(LaptopBundle item)
    {
        return new LaptopBundleView(
            item.Code,
            item.Title,
            item.ExternalRef,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 80),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            Text.Truncate(item.Source, 64),
            item.Summary,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopBundleView> ToLaptopBundleViews(List<LaptopBundle> items)
    {
        var views = new List<LaptopBundleView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopBundleView(item));
        }
        return views;
    }

    public static double SumLaptopBundleView(LaptopBundleView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedGraceDays + (double)parsedAttempts;
    }
}
