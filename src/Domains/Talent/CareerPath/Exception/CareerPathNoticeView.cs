namespace Acme.Workforce.Talent.CareerPath.Exception;

using System.Globalization;

/// <summary>How screens and exports show a career path notice.</summary>
public sealed record CareerPathNoticeView(string Code, string Label, string Ceiling, string Ratio, string Active, string StatusLabel)
{
    public static CareerPathNoticeView ToCareerPathNoticeView(CareerPathNotice item)
    {
        return new CareerPathNoticeView(
            item.Code,
            item.Label,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathNoticeView> ToCareerPathNoticeViews(List<CareerPathNotice> items)
    {
        var views = new List<CareerPathNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathNoticeView(item));
        }
        return views;
    }

    public static double SumCareerPathNoticeView(CareerPathNoticeView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedRatio;
    }
}
