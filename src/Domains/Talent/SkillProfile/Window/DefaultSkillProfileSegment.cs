namespace Acme.Workforce.Talent.SkillProfile.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default skill profile segment is in its life cycle.</summary>
public enum DefaultSkillProfileSegmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One default skill profile segment, with what the planners need to act on it.</summary>
public sealed class DefaultSkillProfileSegment(string code, string title, int startsAt, string source, int retries, DefaultSkillProfileSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int StartsAt { get; set; } = startsAt;
    public string Source { get; set; } = source;
    public int Retries { get; set; } = retries;
    public DefaultSkillProfileSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a default skill profile segment in status new.</summary>
    public static DefaultSkillProfileSegment NewDefaultSkillProfileSegment(string code, string title)
    {
        return new DefaultSkillProfileSegment(code, title, 0, "", 0, DefaultSkillProfileSegmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultSkillProfileSegmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultSkillProfileSegmentStatus next)
    {
        switch (Status)
        {
            case DefaultSkillProfileSegmentStatus.New:
                switch (next)
                {
                    case DefaultSkillProfileSegmentStatus.Done:
                        return true;
                }
                return false;
            case DefaultSkillProfileSegmentStatus.Queued:
                switch (next)
                {
                    case DefaultSkillProfileSegmentStatus.Running:
                    case DefaultSkillProfileSegmentStatus.Done:
                        return true;
                }
                return false;
            case DefaultSkillProfileSegmentStatus.Running:
                return next == DefaultSkillProfileSegmentStatus.Done;
            case DefaultSkillProfileSegmentStatus.Done:
                switch (next)
                {
                    case DefaultSkillProfileSegmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultSkillProfileSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default skill profile segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)StartsAt * (double)Retries, 2);
    }

    public DefaultSkillProfileSegment Copy()
    {
        return new DefaultSkillProfileSegment(Code, Title, StartsAt, Source, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
