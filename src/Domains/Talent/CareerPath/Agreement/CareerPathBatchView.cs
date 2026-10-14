namespace Acme.Workforce.Talent.CareerPath.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path batch.</summary>
public sealed record CareerPathBatchView(string Code, string NameText, string CategoryText, string Summary, string ActiveText, string Minimum, string Title, string CostCenter, string Grade, string Team, string StatusLabel)
{
    public static CareerPathBatchView ToCareerPathBatchView(CareerPathBatch item)
    {
        return new CareerPathBatchView(
            item.Code,
            item.Name,
            item.Category,
            item.Summary,
            $"{item.Active}",
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Title, 40),
            item.CostCenter,
            item.Grade,
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathBatchView> ToCareerPathBatchViews(List<CareerPathBatch> items)
    {
        var views = new List<CareerPathBatchView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathBatchView(item));
        }
        return views;
    }

    public static double SumCareerPathBatchView(CareerPathBatchView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedMinimum;
    }
}
