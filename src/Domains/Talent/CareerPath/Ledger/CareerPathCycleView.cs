namespace Acme.Workforce.Talent.CareerPath.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path cycle.</summary>
public sealed record CareerPathCycleView(string Code, string Name, string Checksum, string DepartmentText, string ContactEmail, string ExpiresAtText, string Maximum, string ManagerId, string ActiveText, string RatingText, string Attempts, string StatusLabel)
{
    public static CareerPathCycleView ToCareerPathCycleView(CareerPathCycle item)
    {
        return new CareerPathCycleView(
            item.Code,
            item.Name,
            item.Checksum,
            item.Department,
            Text.Truncate(item.ContactEmail, 120),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            $"{item.Active}",
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathCycleView> ToCareerPathCycleViews(List<CareerPathCycle> items)
    {
        var views = new List<CareerPathCycleView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathCycleView(item));
        }
        return views;
    }

    public static double SumCareerPathCycleView(CareerPathCycleView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedMaximum + parsedRating;
    }
}
