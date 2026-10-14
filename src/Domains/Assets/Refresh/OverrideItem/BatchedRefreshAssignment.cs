namespace Acme.Workforce.Assets.Refresh.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched refresh assignment is in its life cycle.</summary>
public enum BatchedRefreshAssignmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One batched refresh assignment, with what the planners need to act on it.</summary>
public sealed class BatchedRefreshAssignment(string code, string name, int batchSize, string currency, int windowDays, int retries, string category, string locale, BatchedRefreshAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public string Currency { get; set; } = currency;
    public int WindowDays { get; set; } = windowDays;
    public int Retries { get; set; } = retries;
    public string Category { get; set; } = category;
    public string Locale { get; set; } = locale;
    public BatchedRefreshAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a batched refresh assignment in status new.</summary>
    public static BatchedRefreshAssignment NewBatchedRefreshAssignment(string code, string name)
    {
        return new BatchedRefreshAssignment(code, name, 0, "", 0, 0, "", "", BatchedRefreshAssignmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedRefreshAssignmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedRefreshAssignmentStatus next)
    {
        switch (Status)
        {
            case BatchedRefreshAssignmentStatus.New:
                switch (next)
                {
                    case BatchedRefreshAssignmentStatus.Queued:
                        return true;
                }
                return false;
            case BatchedRefreshAssignmentStatus.Queued:
                return next == BatchedRefreshAssignmentStatus.Done || next == BatchedRefreshAssignmentStatus.Failed;
            case BatchedRefreshAssignmentStatus.Running:
                return next == BatchedRefreshAssignmentStatus.Done;
            case BatchedRefreshAssignmentStatus.Done:
                switch (next)
                {
                    case BatchedRefreshAssignmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedRefreshAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched refresh assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)BatchSize * (double)WindowDays, 2);
    }

    public BatchedRefreshAssignment Copy()
    {
        return new BatchedRefreshAssignment(
            Code,
            Name,
            BatchSize,
            Currency,
            WindowDays,
            Retries,
            Category,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
