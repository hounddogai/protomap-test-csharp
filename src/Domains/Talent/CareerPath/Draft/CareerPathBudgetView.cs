namespace Acme.Workforce.Talent.CareerPath.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path budget.</summary>
public sealed record CareerPathBudgetView(string Code, string Name, string EmployeeIdText, string NotesText, string RatingText, string UpdatedAtText, string Label, string SequenceText, string StatusLabel)
{
    public static CareerPathBudgetView ToCareerPathBudgetView(CareerPathBudget item)
    {
        return new CareerPathBudgetView(
            item.Code,
            Text.Truncate(item.Name, 120),
            Text.Truncate(item.EmployeeId, 80),
            item.Notes,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Label, 80),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathBudgetView> ToCareerPathBudgetViews(List<CareerPathBudget> items)
    {
        var views = new List<CareerPathBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathBudgetView(item));
        }
        return views;
    }

    public static double SumCareerPathBudgetView(CareerPathBudgetView view)
    {
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedUpdatedAt + (double)parsedSequence;
    }
}
