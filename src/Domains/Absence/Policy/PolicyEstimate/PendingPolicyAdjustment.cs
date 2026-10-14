namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending policy adjustment is in its life cycle.</summary>
public enum PendingPolicyAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A pending policy adjustment. Its code is unique within a tenant.</summary>
public sealed class PendingPolicyAdjustment(string code, string title, string externalRef, string location, int batchSize, PendingPolicyAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public string Location { get; set; } = location;
    public int BatchSize { get; set; } = batchSize;
    public PendingPolicyAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a pending policy adjustment in status idle.</summary>
    public static PendingPolicyAdjustment NewPendingPolicyAdjustment(string code, string title)
    {
        return new PendingPolicyAdjustment(code, title, "", "", 0, PendingPolicyAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingPolicyAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingPolicyAdjustmentStatus next)
    {
        switch (Status)
        {
            case PendingPolicyAdjustmentStatus.Idle:
                switch (next)
                {
                    case PendingPolicyAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
            case PendingPolicyAdjustmentStatus.Warming:
                switch (next)
                {
                    case PendingPolicyAdjustmentStatus.Draining:
                        return true;
                }
                return false;
            case PendingPolicyAdjustmentStatus.Ready:
                return next == PendingPolicyAdjustmentStatus.Draining;
            case PendingPolicyAdjustmentStatus.Draining:
                switch (next)
                {
                    case PendingPolicyAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingPolicyAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending policy adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)BatchSize;
    }

    public PendingPolicyAdjustment Copy()
    {
        return new PendingPolicyAdjustment(Code, Title, ExternalRef, Location, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
