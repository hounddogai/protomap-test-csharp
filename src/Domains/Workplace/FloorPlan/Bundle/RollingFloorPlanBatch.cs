namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling floor plan batch is in its life cycle.</summary>
public enum RollingFloorPlanBatchStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One rolling floor plan batch, with what the planners need to act on it.</summary>
public sealed class RollingFloorPlanBatch(string code, string title, string skill, Dictionary<string, string> attributes, int graceDays, int sequence, int version, RollingFloorPlanBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int GraceDays { get; set; } = graceDays;
    public int Sequence { get; set; } = sequence;
    public int Version { get; set; } = version;
    public RollingFloorPlanBatchStatus Status { get; set; } = status;

    /// <summary>Starts a rolling floor plan batch in status requested.</summary>
    public static RollingFloorPlanBatch NewRollingFloorPlanBatch(string code, string title)
    {
        return new RollingFloorPlanBatch(
            code,
            title,
            "",
            new Dictionary<string, string>(),
            0,
            0,
            0,
            RollingFloorPlanBatchStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingFloorPlanBatchStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingFloorPlanBatchStatus next)
    {
        switch (Status)
        {
            case RollingFloorPlanBatchStatus.Requested:
                return next == RollingFloorPlanBatchStatus.Granted;
            case RollingFloorPlanBatchStatus.Granted:
                return next == RollingFloorPlanBatchStatus.Revoked || next == RollingFloorPlanBatchStatus.Expired;
            case RollingFloorPlanBatchStatus.Revoked:
                switch (next)
                {
                    case RollingFloorPlanBatchStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingFloorPlanBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling floor plan batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"rolling floor plan batch"} {Code}";
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

    public double EffectiveLoad()
    {
        return Math.Round((double)Version - (double)Sequence, 2);
    }

    public RollingFloorPlanBatch Copy()
    {
        return new RollingFloorPlanBatch(Code, Title, Skill, Attributes, GraceDays, Sequence, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
