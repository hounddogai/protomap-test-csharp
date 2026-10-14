namespace Acme.Workforce.Assets.Warranty.WarrantyAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched warranty draft is in its life cycle.</summary>
public enum BatchedWarrantyDraftStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A batched warranty draft. Its code is unique within a tenant.</summary>
public sealed class BatchedWarrantyDraft(string code, string title, int durationMinutes, double ceiling, int version, double minimum, string location, BatchedWarrantyDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Ceiling { get; set; } = ceiling;
    public int Version { get; set; } = version;
    public double Minimum { get; set; } = minimum;
    public string Location { get; set; } = location;
    public BatchedWarrantyDraftStatus Status { get; set; } = status;

    /// <summary>Starts a batched warranty draft in status idle.</summary>
    public static BatchedWarrantyDraft NewBatchedWarrantyDraft(string code, string title)
    {
        return new BatchedWarrantyDraft(code, title, 0, 0.0, 0, 0.0, "", BatchedWarrantyDraftStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedWarrantyDraftStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedWarrantyDraftStatus next)
    {
        switch (Status)
        {
            case BatchedWarrantyDraftStatus.Idle:
                return next == BatchedWarrantyDraftStatus.Ready;
            case BatchedWarrantyDraftStatus.Warming:
                switch (next)
                {
                    case BatchedWarrantyDraftStatus.Draining:
                        return true;
                }
                return false;
            case BatchedWarrantyDraftStatus.Ready:
                return next == BatchedWarrantyDraftStatus.Draining || next == BatchedWarrantyDraftStatus.Stopped;
            case BatchedWarrantyDraftStatus.Draining:
                switch (next)
                {
                    case BatchedWarrantyDraftStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedWarrantyDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched warranty draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Version - Minimum, 2);
    }

    public BatchedWarrantyDraft Copy()
    {
        return new BatchedWarrantyDraft(Code, Title, DurationMinutes, Ceiling, Version, Minimum, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
