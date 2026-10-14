namespace Acme.Workforce.Talent.SkillProfile.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive skill profile threshold is in its life cycle.</summary>
public enum AdaptiveSkillProfileThresholdStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a adaptive skill profile threshold and its current status.</summary>
public sealed class AdaptiveSkillProfileThreshold(string code, string title, string skill, int batchSize, int retries, AdaptiveSkillProfileThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public int BatchSize { get; set; } = batchSize;
    public int Retries { get; set; } = retries;
    public AdaptiveSkillProfileThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive skill profile threshold in status open.</summary>
    public static AdaptiveSkillProfileThreshold NewAdaptiveSkillProfileThreshold(string code, string title)
    {
        return new AdaptiveSkillProfileThreshold(code, title, "", 0, 0, AdaptiveSkillProfileThresholdStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveSkillProfileThresholdStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveSkillProfileThresholdStatus next)
    {
        switch (Status)
        {
            case AdaptiveSkillProfileThresholdStatus.Open:
                return next == AdaptiveSkillProfileThresholdStatus.InReview || next == AdaptiveSkillProfileThresholdStatus.Resolved;
            case AdaptiveSkillProfileThresholdStatus.InReview:
                return next == AdaptiveSkillProfileThresholdStatus.Archived;
            case AdaptiveSkillProfileThresholdStatus.Resolved:
                return next == AdaptiveSkillProfileThresholdStatus.Reopened;
            case AdaptiveSkillProfileThresholdStatus.Reopened:
                return next == AdaptiveSkillProfileThresholdStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AdaptiveSkillProfileThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive skill profile threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)BatchSize * (double)Retries, 2);
    }

    public AdaptiveSkillProfileThreshold Copy()
    {
        return new AdaptiveSkillProfileThreshold(Code, Title, Skill, BatchSize, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
