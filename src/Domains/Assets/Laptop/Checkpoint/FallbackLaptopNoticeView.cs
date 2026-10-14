namespace Acme.Workforce.Assets.Laptop.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback laptop notice.</summary>
public sealed record FallbackLaptopNoticeView(string Code, string Label, string Department, string Visible, string Budget, string StatusLabel)
{
    public static FallbackLaptopNoticeView ToFallbackLaptopNoticeView(FallbackLaptopNotice item)
    {
        return new FallbackLaptopNoticeView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Department,
            $"{item.Visible}",
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackLaptopNoticeView> ToFallbackLaptopNoticeViews(List<FallbackLaptopNotice> items)
    {
        var views = new List<FallbackLaptopNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackLaptopNoticeView(item));
        }
        return views;
    }

    public static double SumFallbackLaptopNoticeView(FallbackLaptopNoticeView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedBudget;
    }
}
