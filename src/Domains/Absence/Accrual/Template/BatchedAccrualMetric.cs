namespace Acme.Workforce.Absence.Accrual.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched accrual metric is in its life cycle.</summary>
public enum BatchedAccrualMetricStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One batched accrual metric, with what the planners need to act on it.</summary>
public sealed class BatchedAccrualMetric(string code, string label, bool approved, int revision, int capacity, int sequence, string department, int leaveDays, int retries, BatchedAccrualMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public int Revision { get; set; } = revision;
    public int Capacity { get; set; } = capacity;
    public int Sequence { get; set; } = sequence;
    public string Department { get; set; } = department;
    public int LeaveDays { get; set; } = leaveDays;
    public int Retries { get; set; } = retries;
    public BatchedAccrualMetricStatus Status { get; set; } = status;

    /// <summary>Starts a batched accrual metric in status planned.</summary>
    public static BatchedAccrualMetric NewBatchedAccrualMetric(string code, string label)
    {
        return new BatchedAccrualMetric(code, label, false, 0, 0, 0, "", 0, 0, BatchedAccrualMetricStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedAccrualMetricStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedAccrualMetricStatus next)
    {
        switch (Status)
        {
            case BatchedAccrualMetricStatus.Planned:
                return next == BatchedAccrualMetricStatus.Scheduled || next == BatchedAccrualMetricStatus.InProgress;
            case BatchedAccrualMetricStatus.Scheduled:
                switch (next)
                {
                    case BatchedAccrualMetricStatus.InProgress:
                        return true;
                }
                return false;
            case BatchedAccrualMetricStatus.InProgress:
                return next == BatchedAccrualMetricStatus.Completed;
            case BatchedAccrualMetricStatus.Completed:
                switch (next)
                {
                    case BatchedAccrualMetricStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedAccrualMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched accrual metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"batched accrual metric"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Revision + (double)Capacity, 2);
    }

    public BatchedAccrualMetric Copy()
    {
        return new BatchedAccrualMetric(
            Code,
            Label,
            Approved,
            Revision,
            Capacity,
            Sequence,
            Department,
            LeaveDays,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
