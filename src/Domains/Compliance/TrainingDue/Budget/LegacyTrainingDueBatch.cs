namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy training due batch is in its life cycle.</summary>
public enum LegacyTrainingDueBatchStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A legacy training due batch as the training due team keeps it.</summary>
public sealed class LegacyTrainingDueBatch(string code, string name, string salaryBand, string region, int retries, string phone, LegacyTrainingDueBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public string Region { get; set; } = region;
    public int Retries { get; set; } = retries;
    public string Phone { get; set; } = phone;
    public LegacyTrainingDueBatchStatus Status { get; set; } = status;

    /// <summary>Starts a legacy training due batch in status open.</summary>
    public static LegacyTrainingDueBatch NewLegacyTrainingDueBatch(string code, string name)
    {
        return new LegacyTrainingDueBatch(code, name, "", "", 0, "", LegacyTrainingDueBatchStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyTrainingDueBatchStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyTrainingDueBatchStatus next)
    {
        switch (Status)
        {
            case LegacyTrainingDueBatchStatus.Open:
                return next == LegacyTrainingDueBatchStatus.Archived;
            case LegacyTrainingDueBatchStatus.InReview:
                switch (next)
                {
                    case LegacyTrainingDueBatchStatus.Resolved:
                    case LegacyTrainingDueBatchStatus.Reopened:
                        return true;
                }
                return false;
            case LegacyTrainingDueBatchStatus.Resolved:
                return next == LegacyTrainingDueBatchStatus.Archived;
            case LegacyTrainingDueBatchStatus.Reopened:
                switch (next)
                {
                    case LegacyTrainingDueBatchStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyTrainingDueBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy training due batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public LegacyTrainingDueBatch Copy()
    {
        return new LegacyTrainingDueBatch(Code, Name, SalaryBand, Region, Retries, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
