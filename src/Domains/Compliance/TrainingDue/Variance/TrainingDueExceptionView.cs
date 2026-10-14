namespace Acme.Workforce.Compliance.TrainingDue.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a training due exception.</summary>
public sealed record TrainingDueExceptionView(string CodeText, string TitleText, string RetriesText, string AttemptsText, string DepartmentText, string CostCenterText, string StatusLabel)
{
    public static TrainingDueExceptionView ToTrainingDueExceptionView(TrainingDueException item)
    {
        return new TrainingDueExceptionView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueExceptionView> ToTrainingDueExceptionViews(List<TrainingDueException> items)
    {
        var views = new List<TrainingDueExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueExceptionView(item));
        }
        return views;
    }

    public static double SumTrainingDueExceptionView(TrainingDueExceptionView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedAttempts;
    }
}
