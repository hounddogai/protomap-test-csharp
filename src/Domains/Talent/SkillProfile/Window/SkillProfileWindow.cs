namespace Acme.Workforce.Talent.SkillProfile.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile window is in its life cycle.</summary>
public enum SkillProfileWindowStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A skill profile window as the skill profile team keeps it.</summary>
public sealed class SkillProfileWindow(string code, string name, string segment, bool archived, string description, double score, SkillProfileWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public bool Archived { get; set; } = archived;
    public string Description { get; set; } = description;
    public double Score { get; set; } = score;
    public SkillProfileWindowStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile window in status idle.</summary>
    public static SkillProfileWindow NewSkillProfileWindow(string code, string name)
    {
        return new SkillProfileWindow(code, name, "", false, "", 0.0, SkillProfileWindowStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileWindowStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileWindowStatus next)
    {
        switch (Status)
        {
            case SkillProfileWindowStatus.Idle:
                return next == SkillProfileWindowStatus.Warming;
            case SkillProfileWindowStatus.Warming:
                switch (next)
                {
                    case SkillProfileWindowStatus.Ready:
                    case SkillProfileWindowStatus.Stopped:
                        return true;
                }
                return false;
            case SkillProfileWindowStatus.Ready:
                switch (next)
                {
                    case SkillProfileWindowStatus.Draining:
                    case SkillProfileWindowStatus.Stopped:
                        return true;
                }
                return false;
            case SkillProfileWindowStatus.Draining:
                switch (next)
                {
                    case SkillProfileWindowStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SkillProfileWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public SkillProfileWindow Copy()
    {
        return new SkillProfileWindow(Code, Name, Segment, Archived, Description, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
