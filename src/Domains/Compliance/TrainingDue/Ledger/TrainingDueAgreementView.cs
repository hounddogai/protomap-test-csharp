namespace Acme.Workforce.Compliance.TrainingDue.Ledger;

using System.Globalization;

/// <summary>How screens and exports show a training due agreement.</summary>
public sealed record TrainingDueAgreementView(string CodeText, string Label, string ContactEmailText, string Score, string SourceText, string StatusLabel)
{
    public static TrainingDueAgreementView ToTrainingDueAgreementView(TrainingDueAgreement item)
    {
        return new TrainingDueAgreementView(
            item.Code,
            item.Label,
            item.ContactEmail,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueAgreementView> ToTrainingDueAgreementViews(List<TrainingDueAgreement> items)
    {
        var views = new List<TrainingDueAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueAgreementView(item));
        }
        return views;
    }

    public static double SumTrainingDueAgreementView(TrainingDueAgreementView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
