namespace Acme.Workforce.Compliance.Audit.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global audit checkpoint is in its life cycle.</summary>
public enum GlobalAuditCheckpointStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a global audit checkpoint and its current status.</summary>
public sealed class GlobalAuditCheckpoint(string code, string name, string salaryBand, int capacity, int tenureMonths, string managerId, double minimum, int graceDays, double ratio, string jobCode, GlobalAuditCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public int Capacity { get; set; } = capacity;
    public int TenureMonths { get; set; } = tenureMonths;
    public string ManagerId { get; set; } = managerId;
    public double Minimum { get; set; } = minimum;
    public int GraceDays { get; set; } = graceDays;
    public double Ratio { get; set; } = ratio;
    public string JobCode { get; set; } = jobCode;
    public GlobalAuditCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a global audit checkpoint in status open.</summary>
    public static GlobalAuditCheckpoint NewGlobalAuditCheckpoint(string code, string name)
    {
        return new GlobalAuditCheckpoint(
            code,
            name,
            "",
            0,
            0,
            "",
            0.0,
            0,
            0.0,
            "",
            GlobalAuditCheckpointStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalAuditCheckpointStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalAuditCheckpointStatus next)
    {
        switch (Status)
        {
            case GlobalAuditCheckpointStatus.Open:
                switch (next)
                {
                    case GlobalAuditCheckpointStatus.InReview:
                    case GlobalAuditCheckpointStatus.Resolved:
                        return true;
                }
                return false;
            case GlobalAuditCheckpointStatus.InReview:
                return next == GlobalAuditCheckpointStatus.Reopened || next == GlobalAuditCheckpointStatus.Archived;
            case GlobalAuditCheckpointStatus.Resolved:
                return next == GlobalAuditCheckpointStatus.Reopened || next == GlobalAuditCheckpointStatus.Archived;
            case GlobalAuditCheckpointStatus.Reopened:
                return next == GlobalAuditCheckpointStatus.Archived;
        }
        return false;
    }

    public void MoveTo(GlobalAuditCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global audit checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Capacity - (double)GraceDays, 2);
    }

    public GlobalAuditCheckpoint Copy()
    {
        return new GlobalAuditCheckpoint(
            Code,
            Name,
            SalaryBand,
            Capacity,
            TenureMonths,
            ManagerId,
            Minimum,
            GraceDays,
            Ratio,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
