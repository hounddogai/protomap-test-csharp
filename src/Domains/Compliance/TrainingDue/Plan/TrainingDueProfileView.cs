namespace Acme.Workforce.Compliance.TrainingDue.Plan;

using System.Globalization;

/// <summary>How screens and exports show a training due profile.</summary>
public sealed record TrainingDueProfileView(string Code, string Label, string JobCodeText, string CreatedAtText, string TenureMonths, string CostCenterText, string Minimum, string Retries, string ExpiresAtText, string StatusLabel)
{
    public static TrainingDueProfileView ToTrainingDueProfileView(TrainingDueProfile item)
    {
        return new TrainingDueProfileView(
            item.Code,
            item.Label,
            item.JobCode,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueProfileView> ToTrainingDueProfileViews(List<TrainingDueProfile> items)
    {
        var views = new List<TrainingDueProfileView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueProfileView(item));
        }
        return views;
    }

    public static double SumTrainingDueProfileView(TrainingDueProfileView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedTenureMonths + parsedMinimum;
    }
}
