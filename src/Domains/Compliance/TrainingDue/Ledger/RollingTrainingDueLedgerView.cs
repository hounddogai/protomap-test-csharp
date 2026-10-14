namespace Acme.Workforce.Compliance.TrainingDue.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling training due ledger.</summary>
public sealed record RollingTrainingDueLedgerView(string Code, string Label, string SalaryBand, string Sequence, string Budget, string AttemptsText, string StatusLabel)
{
    public static RollingTrainingDueLedgerView ToRollingTrainingDueLedgerView(RollingTrainingDueLedger item)
    {
        return new RollingTrainingDueLedgerView(
            item.Code,
            Text.Truncate(item.Label, 40),
            Text.Truncate(item.SalaryBand, 120),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingTrainingDueLedgerView> ToRollingTrainingDueLedgerViews(List<RollingTrainingDueLedger> items)
    {
        var views = new List<RollingTrainingDueLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToRollingTrainingDueLedgerView(item));
        }
        return views;
    }

    public static double SumRollingTrainingDueLedgerView(RollingTrainingDueLedgerView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedBudget + (double)parsedAttempts;
    }
}
