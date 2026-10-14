namespace Acme.Workforce.Compliance.TrainingDue.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a training due variance.</summary>
public sealed record TrainingDueVarianceView(string Code, string Name, string Version, string Threshold, string EndsAt, string Capacity, string DepartmentText, string GradeText, string FloorValue, string Skill, string StatusLabel)
{
    public static TrainingDueVarianceView ToTrainingDueVarianceView(TrainingDueVariance item)
    {
        return new TrainingDueVarianceView(
            item.Code,
            item.Name,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 120),
            item.Grade,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueVarianceView> ToTrainingDueVarianceViews(List<TrainingDueVariance> items)
    {
        var views = new List<TrainingDueVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueVarianceView(item));
        }
        return views;
    }

    public static double SumTrainingDueVarianceView(TrainingDueVarianceView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedThreshold + (double)parsedEndsAt;
    }
}
