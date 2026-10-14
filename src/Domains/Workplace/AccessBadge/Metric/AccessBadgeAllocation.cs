namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge allocation is in its life cycle.</summary>
public enum AccessBadgeAllocationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One access badge allocation, with what the planners need to act on it.</summary>
public sealed class AccessBadgeAllocation(string code, string label, int retries, int attempts, string contactEmail, AccessBadgeAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public int Attempts { get; set; } = attempts;
    public string ContactEmail { get; set; } = contactEmail;
    public AccessBadgeAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a access badge allocation in status idle.</summary>
    public static AccessBadgeAllocation NewAccessBadgeAllocation(string code, string label)
    {
        return new AccessBadgeAllocation(code, label, 0, 0, "", AccessBadgeAllocationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeAllocationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeAllocationStatus next)
    {
        switch (Status)
        {
            case AccessBadgeAllocationStatus.Idle:
                switch (next)
                {
                    case AccessBadgeAllocationStatus.Draining:
                        return true;
                }
                return false;
            case AccessBadgeAllocationStatus.Warming:
                switch (next)
                {
                    case AccessBadgeAllocationStatus.Draining:
                    case AccessBadgeAllocationStatus.Stopped:
                        return true;
                }
                return false;
            case AccessBadgeAllocationStatus.Ready:
                return next == AccessBadgeAllocationStatus.Draining;
            case AccessBadgeAllocationStatus.Draining:
                switch (next)
                {
                    case AccessBadgeAllocationStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetVolume()
    {
        return Math.Round((double)Retries * (double)Attempts, 2);
    }

    public AccessBadgeAllocation Copy()
    {
        return new AccessBadgeAllocation(Code, Label, Retries, Attempts, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
