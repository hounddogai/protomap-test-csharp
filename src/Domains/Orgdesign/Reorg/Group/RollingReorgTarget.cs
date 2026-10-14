namespace Acme.Workforce.Orgdesign.Reorg.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling reorg target is in its life cycle.</summary>
public enum RollingReorgTargetStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One rolling reorg target, with what the planners need to act on it.</summary>
public sealed class RollingReorgTarget(string code, string name, int durationMinutes, string description, double ceiling, string phone, int sequence, string badge, string ownerId, int limit, RollingReorgTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Description { get; set; } = description;
    public double Ceiling { get; set; } = ceiling;
    public string Phone { get; set; } = phone;
    public int Sequence { get; set; } = sequence;
    public string Badge { get; set; } = badge;
    public string OwnerId { get; set; } = ownerId;
    public int Limit { get; set; } = limit;
    public RollingReorgTargetStatus Status { get; set; } = status;

    /// <summary>Starts a rolling reorg target in status requested.</summary>
    public static RollingReorgTarget NewRollingReorgTarget(string code, string name)
    {
        return new RollingReorgTarget(code, name, 0, "", 0.0, "", 0, "", "", 0, RollingReorgTargetStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingReorgTargetStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingReorgTargetStatus next)
    {
        switch (Status)
        {
            case RollingReorgTargetStatus.Requested:
                return next == RollingReorgTargetStatus.Granted || next == RollingReorgTargetStatus.Expired;
            case RollingReorgTargetStatus.Granted:
                return next == RollingReorgTargetStatus.Expired;
            case RollingReorgTargetStatus.Revoked:
                return next == RollingReorgTargetStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RollingReorgTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling reorg target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Limit + (double)DurationMinutes, 2);
    }

    public RollingReorgTarget Copy()
    {
        return new RollingReorgTarget(
            Code,
            Name,
            DurationMinutes,
            Description,
            Ceiling,
            Phone,
            Sequence,
            Badge,
            OwnerId,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
