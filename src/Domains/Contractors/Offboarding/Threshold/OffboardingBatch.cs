namespace Acme.Workforce.Contractors.Offboarding.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding batch is in its life cycle.</summary>
public enum OffboardingBatchStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A offboarding batch. Its code is unique within a tenant.</summary>
public sealed class OffboardingBatch(string code, string title, string source, bool visible, int capacity, string region, int quantity, OffboardingBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Source { get; set; } = source;
    public bool Visible { get; set; } = visible;
    public int Capacity { get; set; } = capacity;
    public string Region { get; set; } = region;
    public int Quantity { get; set; } = quantity;
    public OffboardingBatchStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding batch in status proposed.</summary>
    public static OffboardingBatch NewOffboardingBatch(string code, string title)
    {
        return new OffboardingBatch(code, title, "", false, 0, "", 0, OffboardingBatchStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingBatchStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingBatchStatus next)
    {
        switch (Status)
        {
            case OffboardingBatchStatus.Proposed:
                return next == OffboardingBatchStatus.Rejected;
            case OffboardingBatchStatus.Approved:
                return next == OffboardingBatchStatus.Rejected || next == OffboardingBatchStatus.Withdrawn;
            case OffboardingBatchStatus.Rejected:
                return next == OffboardingBatchStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(OffboardingBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Capacity + (double)Quantity, 2);
    }

    public OffboardingBatch Copy()
    {
        return new OffboardingBatch(Code, Title, Source, Visible, Capacity, Region, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
