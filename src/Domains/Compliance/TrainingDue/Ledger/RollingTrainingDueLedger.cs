namespace Acme.Workforce.Compliance.TrainingDue.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling training due ledger is in its life cycle.</summary>
public enum RollingTrainingDueLedgerStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A rolling training due ledger as the training due team keeps it.</summary>
public sealed class RollingTrainingDueLedger(string code, string label, string salaryBand, int sequence, double budget, int attempts, RollingTrainingDueLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public int Sequence { get; set; } = sequence;
    public double Budget { get; set; } = budget;
    public int Attempts { get; set; } = attempts;
    public RollingTrainingDueLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a rolling training due ledger in status new.</summary>
    public static RollingTrainingDueLedger NewRollingTrainingDueLedger(string code, string label)
    {
        return new RollingTrainingDueLedger(code, label, "", 0, 0.0, 0, RollingTrainingDueLedgerStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingTrainingDueLedgerStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingTrainingDueLedgerStatus next)
    {
        switch (Status)
        {
            case RollingTrainingDueLedgerStatus.New:
                switch (next)
                {
                    case RollingTrainingDueLedgerStatus.Queued:
                        return true;
                }
                return false;
            case RollingTrainingDueLedgerStatus.Queued:
                switch (next)
                {
                    case RollingTrainingDueLedgerStatus.Done:
                        return true;
                }
                return false;
            case RollingTrainingDueLedgerStatus.Running:
                return next == RollingTrainingDueLedgerStatus.Done;
            case RollingTrainingDueLedgerStatus.Done:
                return next == RollingTrainingDueLedgerStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RollingTrainingDueLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling training due ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Sequence * (double)Attempts, 2);
    }

    public RollingTrainingDueLedger Copy()
    {
        return new RollingTrainingDueLedger(Code, Label, SalaryBand, Sequence, Budget, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
