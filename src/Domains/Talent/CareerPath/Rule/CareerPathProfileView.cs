namespace Acme.Workforce.Talent.CareerPath.Rule;

using System.Globalization;

/// <summary>How screens and exports show a career path profile.</summary>
public sealed record CareerPathProfileView(string CodeText, string Name, string Phone, string ApprovedText, string Margin, string CreatedAt, string ThresholdText, string Rating, string StatusLabel)
{
    public static CareerPathProfileView ToCareerPathProfileView(CareerPathProfile item)
    {
        return new CareerPathProfileView(
            item.Code,
            item.Name,
            item.Phone,
            $"{item.Approved}",
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathProfileView> ToCareerPathProfileViews(List<CareerPathProfile> items)
    {
        var views = new List<CareerPathProfileView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathProfileView(item));
        }
        return views;
    }

    public static double SumCareerPathProfileView(CareerPathProfileView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedCreatedAt + parsedThreshold;
    }
}
