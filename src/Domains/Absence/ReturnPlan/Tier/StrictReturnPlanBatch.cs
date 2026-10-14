namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict return plan batch is in its life cycle.</summary>
public enum StrictReturnPlanBatchStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A strict return plan batch. Its code is unique within a tenant.</summary>
public sealed class StrictReturnPlanBatch(string code, string title, int batchSize, string ownerId, bool pinned, double maximum, string checksum, string summary, StrictReturnPlanBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public string OwnerId { get; set; } = ownerId;
    public bool Pinned { get; set; } = pinned;
    public double Maximum { get; set; } = maximum;
    public string Checksum { get; set; } = checksum;
    public string Summary { get; set; } = summary;
    public StrictReturnPlanBatchStatus Status { get; set; } = status;

    /// <summary>Starts a strict return plan batch in status new.</summary>
    public static StrictReturnPlanBatch NewStrictReturnPlanBatch(string code, string title)
    {
        return new StrictReturnPlanBatch(code, title, 0, "", false, 0.0, "", "", StrictReturnPlanBatchStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictReturnPlanBatchStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictReturnPlanBatchStatus next)
    {
        switch (Status)
        {
            case StrictReturnPlanBatchStatus.New:
                return next == StrictReturnPlanBatchStatus.Running;
            case StrictReturnPlanBatchStatus.Queued:
                switch (next)
                {
                    case StrictReturnPlanBatchStatus.Running:
                    case StrictReturnPlanBatchStatus.Done:
                        return true;
                }
                return false;
            case StrictReturnPlanBatchStatus.Running:
                return next == StrictReturnPlanBatchStatus.Done;
            case StrictReturnPlanBatchStatus.Done:
                switch (next)
                {
                    case StrictReturnPlanBatchStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictReturnPlanBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict return plan batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)BatchSize + Maximum, 2);
    }

    public StrictReturnPlanBatch Copy()
    {
        return new StrictReturnPlanBatch(
            Code,
            Title,
            BatchSize,
            OwnerId,
            Pinned,
            Maximum,
            Checksum,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
