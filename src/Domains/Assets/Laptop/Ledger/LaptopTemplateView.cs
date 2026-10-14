namespace Acme.Workforce.Assets.Laptop.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a laptop template.</summary>
public sealed record LaptopTemplateView(string CodeText, string Title, string Grade, string Department, string Unit, string ScoreText, string StatusLabel)
{
    public static LaptopTemplateView ToLaptopTemplateView(LaptopTemplate item)
    {
        return new LaptopTemplateView(
            Text.Truncate(item.Code, 120),
            item.Title,
            Text.Truncate(item.Grade, 120),
            item.Department,
            item.Unit,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopTemplateView> ToLaptopTemplateViews(List<LaptopTemplate> items)
    {
        var views = new List<LaptopTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopTemplateView(item));
        }
        return views;
    }

    public static double SumLaptopTemplateView(LaptopTemplateView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
