namespace Acme.Workforce.Engagement.TownHall.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient town hall target is in its life cycle.</summary>
public enum LenientTownHallTargetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A lenient town hall target. Its code is unique within a tenant.</summary>
public sealed class LenientTownHallTarget(string code, string name, string skill, string location, bool approved, List<string> tags, double fte, LenientTownHallTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Skill { get; set; } = skill;
    public string Location { get; set; } = location;
    public bool Approved { get; set; } = approved;
    public List<string> Tags { get; set; } = tags;
    public double Fte { get; set; } = fte;
    public LenientTownHallTargetStatus Status { get; set; } = status;

    /// <summary>Starts a lenient town hall target in status new.</summary>
    public static LenientTownHallTarget NewLenientTownHallTarget(string code, string name)
    {
        return new LenientTownHallTarget(
            code,
            name,
            "",
            "",
            false,
            new List<string>(),
            0.0,
            LenientTownHallTargetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientTownHallTargetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientTownHallTargetStatus next)
    {
        switch (Status)
        {
            case LenientTownHallTargetStatus.New:
                return next == LenientTownHallTargetStatus.Running;
            case LenientTownHallTargetStatus.Queued:
                return next == LenientTownHallTargetStatus.Running;
            case LenientTownHallTargetStatus.Running:
                switch (next)
                {
                    case LenientTownHallTargetStatus.Done:
                    case LenientTownHallTargetStatus.Failed:
                        return true;
                }
                return false;
            case LenientTownHallTargetStatus.Done:
                switch (next)
                {
                    case LenientTownHallTargetStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientTownHallTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient town hall target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient town hall target"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public LenientTownHallTarget Copy()
    {
        return new LenientTownHallTarget(Code, Name, Skill, Location, Approved, Tags, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
