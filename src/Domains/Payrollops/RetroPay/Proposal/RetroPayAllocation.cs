namespace Acme.Workforce.Payrollops.RetroPay.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay allocation is in its life cycle.</summary>
public enum RetroPayAllocationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A retro pay allocation. Its code is unique within a tenant.</summary>
public sealed class RetroPayAllocation(string code, string label, int priority, string category, int version, int endsAt, string reviewer, string description, RetroPayAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public string Category { get; set; } = category;
    public int Version { get; set; } = version;
    public int EndsAt { get; set; } = endsAt;
    public string Reviewer { get; set; } = reviewer;
    public string Description { get; set; } = description;
    public RetroPayAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay allocation in status requested.</summary>
    public static RetroPayAllocation NewRetroPayAllocation(string code, string label)
    {
        return new RetroPayAllocation(code, label, 0, "", 0, 0, "", "", RetroPayAllocationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayAllocationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayAllocationStatus next)
    {
        switch (Status)
        {
            case RetroPayAllocationStatus.Requested:
                switch (next)
                {
                    case RetroPayAllocationStatus.Granted:
                    case RetroPayAllocationStatus.Expired:
                        return true;
                }
                return false;
            case RetroPayAllocationStatus.Granted:
                switch (next)
                {
                    case RetroPayAllocationStatus.Revoked:
                    case RetroPayAllocationStatus.Expired:
                        return true;
                }
                return false;
            case RetroPayAllocationStatus.Revoked:
                return next == RetroPayAllocationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RetroPayAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"retro pay allocation"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Version * (double)Priority, 2);
    }

    public RetroPayAllocation Copy()
    {
        return new RetroPayAllocation(
            Code,
            Label,
            Priority,
            Category,
            Version,
            EndsAt,
            Reviewer,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
