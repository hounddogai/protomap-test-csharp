namespace Acme.Workforce.Talent.Pool.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy pool target is in its life cycle.</summary>
public enum LegacyPoolTargetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One legacy pool target, with what the planners need to act on it.</summary>
public sealed class LegacyPoolTarget(string code, string name, string reviewer, int revision, string checksum, bool approved, int priority, double margin, double fte, int startsAt, LegacyPoolTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Reviewer { get; set; } = reviewer;
    public int Revision { get; set; } = revision;
    public string Checksum { get; set; } = checksum;
    public bool Approved { get; set; } = approved;
    public int Priority { get; set; } = priority;
    public double Margin { get; set; } = margin;
    public double Fte { get; set; } = fte;
    public int StartsAt { get; set; } = startsAt;
    public LegacyPoolTargetStatus Status { get; set; } = status;

    /// <summary>Starts a legacy pool target in status idle.</summary>
    public static LegacyPoolTarget NewLegacyPoolTarget(string code, string name)
    {
        return new LegacyPoolTarget(code, name, "", 0, "", false, 0, 0.0, 0.0, 0, LegacyPoolTargetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPoolTargetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPoolTargetStatus next)
    {
        switch (Status)
        {
            case LegacyPoolTargetStatus.Idle:
                switch (next)
                {
                    case LegacyPoolTargetStatus.Warming:
                        return true;
                }
                return false;
            case LegacyPoolTargetStatus.Warming:
                return next == LegacyPoolTargetStatus.Ready || next == LegacyPoolTargetStatus.Draining;
            case LegacyPoolTargetStatus.Ready:
                return next == LegacyPoolTargetStatus.Draining || next == LegacyPoolTargetStatus.Stopped;
            case LegacyPoolTargetStatus.Draining:
                switch (next)
                {
                    case LegacyPoolTargetStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyPoolTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy pool target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Revision - Fte, 2);
    }

    public LegacyPoolTarget Copy()
    {
        return new LegacyPoolTarget(
            Code,
            Name,
            Reviewer,
            Revision,
            Checksum,
            Approved,
            Priority,
            Margin,
            Fte,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
