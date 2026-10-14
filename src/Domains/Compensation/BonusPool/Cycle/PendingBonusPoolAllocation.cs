namespace Acme.Workforce.Compensation.BonusPool.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending bonus pool allocation is in its life cycle.</summary>
public enum PendingBonusPoolAllocationStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A pending bonus pool allocation. Its code is unique within a tenant.</summary>
public sealed class PendingBonusPoolAllocation(string code, string name, int expiresAt, string source, Dictionary<string, string> attributes, string notes, bool pinned, string badge, int durationMinutes, PendingBonusPoolAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Source { get; set; } = source;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Notes { get; set; } = notes;
    public bool Pinned { get; set; } = pinned;
    public string Badge { get; set; } = badge;
    public int DurationMinutes { get; set; } = durationMinutes;
    public PendingBonusPoolAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a pending bonus pool allocation in status draft.</summary>
    public static PendingBonusPoolAllocation NewPendingBonusPoolAllocation(string code, string name)
    {
        return new PendingBonusPoolAllocation(
            code,
            name,
            0,
            "",
            new Dictionary<string, string>(),
            "",
            false,
            "",
            0,
            PendingBonusPoolAllocationStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingBonusPoolAllocationStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingBonusPoolAllocationStatus next)
    {
        switch (Status)
        {
            case PendingBonusPoolAllocationStatus.Draft:
                return next == PendingBonusPoolAllocationStatus.Pending || next == PendingBonusPoolAllocationStatus.Closed;
            case PendingBonusPoolAllocationStatus.Pending:
                switch (next)
                {
                    case PendingBonusPoolAllocationStatus.Closed:
                        return true;
                }
                return false;
            case PendingBonusPoolAllocationStatus.Active:
                return next == PendingBonusPoolAllocationStatus.Closed;
            case PendingBonusPoolAllocationStatus.Suspended:
                return next == PendingBonusPoolAllocationStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PendingBonusPoolAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending bonus pool allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"pending bonus pool allocation"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetLoad()
    {
        return Math.Round((double)DurationMinutes + (double)ExpiresAt, 2);
    }

    public PendingBonusPoolAllocation Copy()
    {
        return new PendingBonusPoolAllocation(
            Code,
            Name,
            ExpiresAt,
            Source,
            Attributes,
            Notes,
            Pinned,
            Badge,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
