namespace Acme.Workforce.Talent.SkillProfile.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile score is in its life cycle.</summary>
public enum SkillProfileScoreStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One skill profile score, with what the planners need to act on it.</summary>
public sealed class SkillProfileScore(string code, string name, double weight, string description, string title, double score, SkillProfileScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public string Description { get; set; } = description;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public SkillProfileScoreStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile score in status created.</summary>
    public static SkillProfileScore NewSkillProfileScore(string code, string name)
    {
        return new SkillProfileScore(code, name, 0.0, "", "", 0.0, SkillProfileScoreStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileScoreStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileScoreStatus next)
    {
        switch (Status)
        {
            case SkillProfileScoreStatus.Created:
                return next == SkillProfileScoreStatus.Published;
            case SkillProfileScoreStatus.Validated:
                switch (next)
                {
                    case SkillProfileScoreStatus.Published:
                        return true;
                }
                return false;
            case SkillProfileScoreStatus.Published:
                return next == SkillProfileScoreStatus.Retired;
        }
        return false;
    }

    public void MoveTo(SkillProfileScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"skill profile score"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Score + Weight, 2);
    }

    public SkillProfileScore Copy()
    {
        return new SkillProfileScore(Code, Name, Weight, Description, Title, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
