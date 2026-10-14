namespace Acme.Workforce.Contractors.Offboarding.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled offboarding batch is in its life cycle.</summary>
public enum ScheduledOffboardingBatchStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a scheduled offboarding batch and its current status.</summary>
public sealed class ScheduledOffboardingBatch(string code, string name, string grade, string employeeId, int priority, int batchSize, bool visible, double ratio, int leaveDays, int attempts, ScheduledOffboardingBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public string EmployeeId { get; set; } = employeeId;
    public int Priority { get; set; } = priority;
    public int BatchSize { get; set; } = batchSize;
    public bool Visible { get; set; } = visible;
    public double Ratio { get; set; } = ratio;
    public int LeaveDays { get; set; } = leaveDays;
    public int Attempts { get; set; } = attempts;
    public ScheduledOffboardingBatchStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled offboarding batch in status created.</summary>
    public static ScheduledOffboardingBatch NewScheduledOffboardingBatch(string code, string name)
    {
        return new ScheduledOffboardingBatch(
            code,
            name,
            "",
            "",
            0,
            0,
            false,
            0.0,
            0,
            0,
            ScheduledOffboardingBatchStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledOffboardingBatchStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledOffboardingBatchStatus next)
    {
        switch (Status)
        {
            case ScheduledOffboardingBatchStatus.Created:
                return next == ScheduledOffboardingBatchStatus.Validated || next == ScheduledOffboardingBatchStatus.Published;
            case ScheduledOffboardingBatchStatus.Validated:
                switch (next)
                {
                    case ScheduledOffboardingBatchStatus.Published:
                        return true;
                }
                return false;
            case ScheduledOffboardingBatchStatus.Published:
                return next == ScheduledOffboardingBatchStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ScheduledOffboardingBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled offboarding batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled offboarding batch"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Attempts + (double)Priority, 2);
    }

    public ScheduledOffboardingBatch Copy()
    {
        return new ScheduledOffboardingBatch(
            Code,
            Name,
            Grade,
            EmployeeId,
            Priority,
            BatchSize,
            Visible,
            Ratio,
            LeaveDays,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
