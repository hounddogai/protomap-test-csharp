namespace Acme.Workforce.Assets.Refresh.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched refresh metric is in its life cycle.</summary>
public enum BatchedRefreshMetricStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A batched refresh metric. Its code is unique within a tenant.</summary>
public sealed class BatchedRefreshMetric(string code, string title, string notes, string category, double amount, int capacity, string skill, int limit, int batchSize, string contactEmail, BatchedRefreshMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public string Category { get; set; } = category;
    public double Amount { get; set; } = amount;
    public int Capacity { get; set; } = capacity;
    public string Skill { get; set; } = skill;
    public int Limit { get; set; } = limit;
    public int BatchSize { get; set; } = batchSize;
    public string ContactEmail { get; set; } = contactEmail;
    public BatchedRefreshMetricStatus Status { get; set; } = status;

    /// <summary>Starts a batched refresh metric in status planned.</summary>
    public static BatchedRefreshMetric NewBatchedRefreshMetric(string code, string title)
    {
        return new BatchedRefreshMetric(
            code,
            title,
            "",
            "",
            0.0,
            0,
            "",
            0,
            0,
            "",
            BatchedRefreshMetricStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedRefreshMetricStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedRefreshMetricStatus next)
    {
        switch (Status)
        {
            case BatchedRefreshMetricStatus.Planned:
                switch (next)
                {
                    case BatchedRefreshMetricStatus.InProgress:
                        return true;
                }
                return false;
            case BatchedRefreshMetricStatus.Scheduled:
                return next == BatchedRefreshMetricStatus.Completed || next == BatchedRefreshMetricStatus.Cancelled;
            case BatchedRefreshMetricStatus.InProgress:
                switch (next)
                {
                    case BatchedRefreshMetricStatus.Cancelled:
                        return true;
                }
                return false;
            case BatchedRefreshMetricStatus.Completed:
                return next == BatchedRefreshMetricStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BatchedRefreshMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched refresh metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Limit * (double)Capacity, 2);
    }

    public BatchedRefreshMetric Copy()
    {
        return new BatchedRefreshMetric(
            Code,
            Title,
            Notes,
            Category,
            Amount,
            Capacity,
            Skill,
            Limit,
            BatchSize,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
