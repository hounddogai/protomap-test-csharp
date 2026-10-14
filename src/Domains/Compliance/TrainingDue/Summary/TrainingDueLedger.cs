namespace Acme.Workforce.Compliance.TrainingDue.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due ledger is in its life cycle.</summary>
public enum TrainingDueLedgerStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A training due ledger as the training due team keeps it.</summary>
public sealed class TrainingDueLedger(string code, string name, string contactEmail, string region, int graceDays, double rating, double threshold, string timezone, string unit, double maximum, double discount, TrainingDueLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public string Region { get; set; } = region;
    public int GraceDays { get; set; } = graceDays;
    public double Rating { get; set; } = rating;
    public double Threshold { get; set; } = threshold;
    public string Timezone { get; set; } = timezone;
    public string Unit { get; set; } = unit;
    public double Maximum { get; set; } = maximum;
    public double Discount { get; set; } = discount;
    public TrainingDueLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a training due ledger in status open.</summary>
    public static TrainingDueLedger NewTrainingDueLedger(string code, string name)
    {
        return new TrainingDueLedger(
            code,
            name,
            "",
            "",
            0,
            0.0,
            0.0,
            "",
            "",
            0.0,
            0.0,
            TrainingDueLedgerStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueLedgerStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueLedgerStatus next)
    {
        switch (Status)
        {
            case TrainingDueLedgerStatus.Open:
                switch (next)
                {
                    case TrainingDueLedgerStatus.Reopened:
                        return true;
                }
                return false;
            case TrainingDueLedgerStatus.InReview:
                return next == TrainingDueLedgerStatus.Resolved || next == TrainingDueLedgerStatus.Archived;
            case TrainingDueLedgerStatus.Resolved:
                return next == TrainingDueLedgerStatus.Archived;
            case TrainingDueLedgerStatus.Reopened:
                switch (next)
                {
                    case TrainingDueLedgerStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TrainingDueLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"training due ledger"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Discount + (double)GraceDays, 2);
    }

    public TrainingDueLedger Copy()
    {
        return new TrainingDueLedger(
            Code,
            Name,
            ContactEmail,
            Region,
            GraceDays,
            Rating,
            Threshold,
            Timezone,
            Unit,
            Maximum,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
