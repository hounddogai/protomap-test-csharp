namespace Acme.Workforce.Workplace.Visitor.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor bundle.</summary>
public sealed record VisitorBundleView(string Code, string NameText, string UpdatedAt, string TitleText, string Rate, string Minimum, string EmployeeIdText, string ScoreText, string StatusLabel)
{
    public static VisitorBundleView ToVisitorBundleView(VisitorBundle item)
    {
        return new VisitorBundleView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 40),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorBundleView> ToVisitorBundleViews(List<VisitorBundle> items)
    {
        var views = new List<VisitorBundleView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorBundleView(item));
        }
        return views;
    }

    public static double SumVisitorBundleView(VisitorBundleView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedRate + parsedMinimum;
    }
}
