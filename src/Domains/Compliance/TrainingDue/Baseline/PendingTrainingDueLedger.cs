namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending training due ledger is in its life cycle.</summary>
public enum PendingTrainingDueLedgerStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A pending training due ledger. Its code is unique within a tenant.</summary>
public sealed class PendingTrainingDueLedger(string code, string name, int retries, double rating, string segment, string description, PendingTrainingDueLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public double Rating { get; set; } = rating;
    public string Segment { get; set; } = segment;
    public string Description { get; set; } = description;
    public PendingTrainingDueLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a pending training due ledger in status created.</summary>
    public static PendingTrainingDueLedger NewPendingTrainingDueLedger(string code, string name)
    {
        return new PendingTrainingDueLedger(code, name, 0, 0.0, "", "", PendingTrainingDueLedgerStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingTrainingDueLedgerStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingTrainingDueLedgerStatus next)
    {
        switch (Status)
        {
            case PendingTrainingDueLedgerStatus.Created:
                return next == PendingTrainingDueLedgerStatus.Retired;
            case PendingTrainingDueLedgerStatus.Validated:
                return next == PendingTrainingDueLedgerStatus.Published || next == PendingTrainingDueLedgerStatus.Retired;
            case PendingTrainingDueLedgerStatus.Published:
                switch (next)
                {
                    case PendingTrainingDueLedgerStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingTrainingDueLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending training due ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pending training due ledger"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Retries * Rating, 2);
    }

    public PendingTrainingDueLedger Copy()
    {
        return new PendingTrainingDueLedger(Code, Name, Retries, Rating, Segment, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
