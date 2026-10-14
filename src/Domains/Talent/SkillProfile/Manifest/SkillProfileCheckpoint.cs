namespace Acme.Workforce.Talent.SkillProfile.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile checkpoint is in its life cycle.</summary>
public enum SkillProfileCheckpointStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A skill profile checkpoint as the skill profile team keeps it.</summary>
public sealed class SkillProfileCheckpoint(string code, string title, double rate, int graceDays, Dictionary<string, string> attributes, string costCenter, string segment, SkillProfileCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public int GraceDays { get; set; } = graceDays;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string CostCenter { get; set; } = costCenter;
    public string Segment { get; set; } = segment;
    public SkillProfileCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile checkpoint in status planned.</summary>
    public static SkillProfileCheckpoint NewSkillProfileCheckpoint(string code, string title)
    {
        return new SkillProfileCheckpoint(
            code,
            title,
            0.0,
            0,
            new Dictionary<string, string>(),
            "",
            "",
            SkillProfileCheckpointStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileCheckpointStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileCheckpointStatus next)
    {
        switch (Status)
        {
            case SkillProfileCheckpointStatus.Planned:
                switch (next)
                {
                    case SkillProfileCheckpointStatus.Completed:
                        return true;
                }
                return false;
            case SkillProfileCheckpointStatus.Scheduled:
                return next == SkillProfileCheckpointStatus.Cancelled;
            case SkillProfileCheckpointStatus.InProgress:
                switch (next)
                {
                    case SkillProfileCheckpointStatus.Completed:
                        return true;
                }
                return false;
            case SkillProfileCheckpointStatus.Completed:
                return next == SkillProfileCheckpointStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SkillProfileCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
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
        return Math.Round((double)GraceDays * Rate, 2);
    }

    public SkillProfileCheckpoint Copy()
    {
        return new SkillProfileCheckpoint(Code, Title, Rate, GraceDays, Attributes, CostCenter, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
