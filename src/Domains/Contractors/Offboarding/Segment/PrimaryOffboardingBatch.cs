namespace Acme.Workforce.Contractors.Offboarding.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary offboarding batch is in its life cycle.</summary>
public enum PrimaryOffboardingBatchStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A primary offboarding batch. Its code is unique within a tenant.</summary>
public sealed class PrimaryOffboardingBatch(string code, string name, int limit, string reviewer, int priority, int sequence, double fte, int leaveDays, PrimaryOffboardingBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public string Reviewer { get; set; } = reviewer;
    public int Priority { get; set; } = priority;
    public int Sequence { get; set; } = sequence;
    public double Fte { get; set; } = fte;
    public int LeaveDays { get; set; } = leaveDays;
    public PrimaryOffboardingBatchStatus Status { get; set; } = status;

    /// <summary>Starts a primary offboarding batch in status draft.</summary>
    public static PrimaryOffboardingBatch NewPrimaryOffboardingBatch(string code, string name)
    {
        return new PrimaryOffboardingBatch(code, name, 0, "", 0, 0, 0.0, 0, PrimaryOffboardingBatchStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryOffboardingBatchStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryOffboardingBatchStatus next)
    {
        switch (Status)
        {
            case PrimaryOffboardingBatchStatus.Draft:
                return next == PrimaryOffboardingBatchStatus.Suspended;
            case PrimaryOffboardingBatchStatus.Pending:
                switch (next)
                {
                    case PrimaryOffboardingBatchStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryOffboardingBatchStatus.Active:
                switch (next)
                {
                    case PrimaryOffboardingBatchStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryOffboardingBatchStatus.Suspended:
                switch (next)
                {
                    case PrimaryOffboardingBatchStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryOffboardingBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary offboarding batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Fte - (double)Priority, 2);
    }

    public PrimaryOffboardingBatch Copy()
    {
        return new PrimaryOffboardingBatch(Code, Name, Limit, Reviewer, Priority, Sequence, Fte, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
