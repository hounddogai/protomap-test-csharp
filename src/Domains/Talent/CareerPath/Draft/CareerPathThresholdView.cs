namespace Acme.Workforce.Talent.CareerPath.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path threshold.</summary>
public sealed record CareerPathThresholdView(string CodeText, string Label, string Approved, string Badge, string CreatedAt, string EndsAt, string Summary, string StatusLabel)
{
    public static CareerPathThresholdView ToCareerPathThresholdView(CareerPathThreshold item)
    {
        return new CareerPathThresholdView(
            Text.Truncate(item.Code, 120),
            item.Label,
            $"{item.Approved}",
            item.Badge,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathThresholdView> ToCareerPathThresholdViews(List<CareerPathThreshold> items)
    {
        var views = new List<CareerPathThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathThresholdView(item));
        }
        return views;
    }

    public static double SumCareerPathThresholdView(CareerPathThresholdView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedEndsAt;
    }
}
