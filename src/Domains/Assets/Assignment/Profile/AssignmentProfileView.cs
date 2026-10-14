namespace Acme.Workforce.Assets.Assignment.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a assignment profile.</summary>
public sealed record AssignmentProfileView(string Code, string Name, string WindowDays, string CategoryText, string Minimum, string Score, string Cost, string Unit, string BatchSize, string StatusLabel)
{
    public static AssignmentProfileView ToAssignmentProfileView(AssignmentProfile item)
    {
        return new AssignmentProfileView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssignmentProfileView> ToAssignmentProfileViews(List<AssignmentProfile> items)
    {
        var views = new List<AssignmentProfileView>();
        foreach (var item in items)
        {
            views.Add(ToAssignmentProfileView(item));
        }
        return views;
    }

    public static double SumAssignmentProfileView(AssignmentProfileView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedMinimum + parsedScore;
    }
}
