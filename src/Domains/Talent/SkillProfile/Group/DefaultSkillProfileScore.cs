namespace Acme.Workforce.Talent.SkillProfile.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default skill profile score is in its life cycle.</summary>
public enum DefaultSkillProfileScoreStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a default skill profile score and its current status.</summary>
public sealed class DefaultSkillProfileScore(string code, string label, string skill, double discount, int expiresAt, string title, DefaultSkillProfileScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public double Discount { get; set; } = discount;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Title { get; set; } = title;
    public DefaultSkillProfileScoreStatus Status { get; set; } = status;

    /// <summary>Starts a default skill profile score in status idle.</summary>
    public static DefaultSkillProfileScore NewDefaultSkillProfileScore(string code, string label)
    {
        return new DefaultSkillProfileScore(code, label, "", 0.0, 0, "", DefaultSkillProfileScoreStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultSkillProfileScoreStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultSkillProfileScoreStatus next)
    {
        switch (Status)
        {
            case DefaultSkillProfileScoreStatus.Idle:
                switch (next)
                {
                    case DefaultSkillProfileScoreStatus.Warming:
                    case DefaultSkillProfileScoreStatus.Stopped:
                        return true;
                }
                return false;
            case DefaultSkillProfileScoreStatus.Warming:
                switch (next)
                {
                    case DefaultSkillProfileScoreStatus.Ready:
                    case DefaultSkillProfileScoreStatus.Draining:
                        return true;
                }
                return false;
            case DefaultSkillProfileScoreStatus.Ready:
                return next == DefaultSkillProfileScoreStatus.Draining || next == DefaultSkillProfileScoreStatus.Stopped;
            case DefaultSkillProfileScoreStatus.Draining:
                return next == DefaultSkillProfileScoreStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(DefaultSkillProfileScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default skill profile score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"default skill profile score"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)ExpiresAt * Discount, 2);
    }

    public DefaultSkillProfileScore Copy()
    {
        return new DefaultSkillProfileScore(Code, Label, Skill, Discount, ExpiresAt, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
