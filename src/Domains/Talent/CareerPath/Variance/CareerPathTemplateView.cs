namespace Acme.Workforce.Talent.CareerPath.Variance;

using System.Globalization;

/// <summary>How screens and exports show a career path template.</summary>
public sealed record CareerPathTemplateView(string Code, string NameText, string Rate, string WindowDays, string CreatedAt, string StatusLabel)
{
    public static CareerPathTemplateView ToCareerPathTemplateView(CareerPathTemplate item)
    {
        return new CareerPathTemplateView(
            item.Code,
            item.Name,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathTemplateView> ToCareerPathTemplateViews(List<CareerPathTemplate> items)
    {
        var views = new List<CareerPathTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathTemplateView(item));
        }
        return views;
    }

    public static double SumCareerPathTemplateView(CareerPathTemplateView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedWindowDays + (double)parsedCreatedAt;
    }
}
