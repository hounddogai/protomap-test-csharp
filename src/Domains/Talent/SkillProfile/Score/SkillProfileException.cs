namespace Acme.Workforce.Talent.SkillProfile.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile exception is in its life cycle.</summary>
public enum SkillProfileExceptionStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A skill profile exception. Its code is unique within a tenant.</summary>
public sealed class SkillProfileException(string code, string title, List<string> tags, string description, int startsAt, double threshold, int attempts, double weight, SkillProfileExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public string Description { get; set; } = description;
    public int StartsAt { get; set; } = startsAt;
    public double Threshold { get; set; } = threshold;
    public int Attempts { get; set; } = attempts;
    public double Weight { get; set; } = weight;
    public SkillProfileExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile exception in status idle.</summary>
    public static SkillProfileException NewSkillProfileException(string code, string title)
    {
        return new SkillProfileException(
            code,
            title,
            new List<string>(),
            "",
            0,
            0.0,
            0,
            0.0,
            SkillProfileExceptionStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileExceptionStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileExceptionStatus next)
    {
        switch (Status)
        {
            case SkillProfileExceptionStatus.Idle:
                switch (next)
                {
                    case SkillProfileExceptionStatus.Ready:
                        return true;
                }
                return false;
            case SkillProfileExceptionStatus.Warming:
                switch (next)
                {
                    case SkillProfileExceptionStatus.Draining:
                        return true;
                }
                return false;
            case SkillProfileExceptionStatus.Ready:
                switch (next)
                {
                    case SkillProfileExceptionStatus.Stopped:
                        return true;
                }
                return false;
            case SkillProfileExceptionStatus.Draining:
                return next == SkillProfileExceptionStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SkillProfileExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"skill profile exception"} {Code}";
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

    public double AdjustedTotal()
    {
        return Math.Round(Weight * Threshold, 2);
    }

    public SkillProfileException Copy()
    {
        return new SkillProfileException(
            Code,
            Title,
            Tags,
            Description,
            StartsAt,
            Threshold,
            Attempts,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
