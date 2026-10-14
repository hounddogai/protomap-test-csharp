namespace Acme.Workforce.Assets.Laptop.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback laptop template.</summary>
public sealed record FallbackLaptopTemplateView(string CodeText, string Label, string Title, string ActiveText, string StartsAtText, string StatusLabel)
{
    public static FallbackLaptopTemplateView ToFallbackLaptopTemplateView(FallbackLaptopTemplate item)
    {
        return new FallbackLaptopTemplateView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Title,
            $"{item.Active}",
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackLaptopTemplateView> ToFallbackLaptopTemplateViews(List<FallbackLaptopTemplate> items)
    {
        var views = new List<FallbackLaptopTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackLaptopTemplateView(item));
        }
        return views;
    }

    public static double SumFallbackLaptopTemplateView(FallbackLaptopTemplateView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt;
    }
}
