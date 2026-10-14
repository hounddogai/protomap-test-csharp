namespace Acme.Workforce.Talent.SkillProfile.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile request is in its life cycle.</summary>
public enum SkillProfileRequestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One skill profile request, with what the planners need to act on it.</summary>
public sealed class SkillProfileRequest(string code, string name, int attempts, double floorValue, string grade, string badge, int expiresAt, string skill, SkillProfileRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public double FloorValue { get; set; } = floorValue;
    public string Grade { get; set; } = grade;
    public string Badge { get; set; } = badge;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Skill { get; set; } = skill;
    public SkillProfileRequestStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile request in status created.</summary>
    public static SkillProfileRequest NewSkillProfileRequest(string code, string name)
    {
        return new SkillProfileRequest(code, name, 0, 0.0, "", "", 0, "", SkillProfileRequestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileRequestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileRequestStatus next)
    {
        switch (Status)
        {
            case SkillProfileRequestStatus.Created:
                switch (next)
                {
                    case SkillProfileRequestStatus.Validated:
                        return true;
                }
                return false;
            case SkillProfileRequestStatus.Validated:
                switch (next)
                {
                    case SkillProfileRequestStatus.Published:
                        return true;
                }
                return false;
            case SkillProfileRequestStatus.Published:
                return next == SkillProfileRequestStatus.Retired;
        }
        return false;
    }

    public void MoveTo(SkillProfileRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(FloorValue * (double)ExpiresAt, 2);
    }

    public SkillProfileRequest Copy()
    {
        return new SkillProfileRequest(Code, Name, Attempts, FloorValue, Grade, Badge, ExpiresAt, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
