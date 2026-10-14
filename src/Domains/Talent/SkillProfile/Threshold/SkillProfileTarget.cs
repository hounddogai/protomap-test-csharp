namespace Acme.Workforce.Talent.SkillProfile.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile target is in its life cycle.</summary>
public enum SkillProfileTargetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A skill profile target. Its code is unique within a tenant.</summary>
public sealed class SkillProfileTarget(string code, string label, double minimum, int endsAt, int attempts, string badge, int batchSize, int priority, string unit, string ownerId, string region, SkillProfileTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public int EndsAt { get; set; } = endsAt;
    public int Attempts { get; set; } = attempts;
    public string Badge { get; set; } = badge;
    public int BatchSize { get; set; } = batchSize;
    public int Priority { get; set; } = priority;
    public string Unit { get; set; } = unit;
    public string OwnerId { get; set; } = ownerId;
    public string Region { get; set; } = region;
    public SkillProfileTargetStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile target in status created.</summary>
    public static SkillProfileTarget NewSkillProfileTarget(string code, string label)
    {
        return new SkillProfileTarget(
            code,
            label,
            0.0,
            0,
            0,
            "",
            0,
            0,
            "",
            "",
            "",
            SkillProfileTargetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileTargetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileTargetStatus next)
    {
        switch (Status)
        {
            case SkillProfileTargetStatus.Created:
                switch (next)
                {
                    case SkillProfileTargetStatus.Retired:
                        return true;
                }
                return false;
            case SkillProfileTargetStatus.Validated:
                return next == SkillProfileTargetStatus.Retired;
            case SkillProfileTargetStatus.Published:
                return next == SkillProfileTargetStatus.Retired;
        }
        return false;
    }

    public void MoveTo(SkillProfileTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"skill profile target"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Attempts * (double)BatchSize, 2);
    }

    public SkillProfileTarget Copy()
    {
        return new SkillProfileTarget(
            Code,
            Label,
            Minimum,
            EndsAt,
            Attempts,
            Badge,
            BatchSize,
            Priority,
            Unit,
            OwnerId,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
