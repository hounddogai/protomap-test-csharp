namespace Acme.Workforce.Talent.SkillProfile.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default skill profile policy is in its life cycle.</summary>
public enum DefaultSkillProfilePolicyStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One default skill profile policy, with what the planners need to act on it.</summary>
public sealed class DefaultSkillProfilePolicy(string code, string label, int updatedAt, double rating, int leaveDays, int attempts, DefaultSkillProfilePolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Rating { get; set; } = rating;
    public int LeaveDays { get; set; } = leaveDays;
    public int Attempts { get; set; } = attempts;
    public DefaultSkillProfilePolicyStatus Status { get; set; } = status;

    /// <summary>Starts a default skill profile policy in status requested.</summary>
    public static DefaultSkillProfilePolicy NewDefaultSkillProfilePolicy(string code, string label)
    {
        return new DefaultSkillProfilePolicy(code, label, 0, 0.0, 0, 0, DefaultSkillProfilePolicyStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultSkillProfilePolicyStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultSkillProfilePolicyStatus next)
    {
        switch (Status)
        {
            case DefaultSkillProfilePolicyStatus.Requested:
                return next == DefaultSkillProfilePolicyStatus.Revoked;
            case DefaultSkillProfilePolicyStatus.Granted:
                switch (next)
                {
                    case DefaultSkillProfilePolicyStatus.Revoked:
                    case DefaultSkillProfilePolicyStatus.Expired:
                        return true;
                }
                return false;
            case DefaultSkillProfilePolicyStatus.Revoked:
                return next == DefaultSkillProfilePolicyStatus.Expired;
        }
        return false;
    }

    public void MoveTo(DefaultSkillProfilePolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default skill profile policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedValue()
    {
        return Math.Round(Rating + (double)LeaveDays, 2);
    }

    public DefaultSkillProfilePolicy Copy()
    {
        return new DefaultSkillProfilePolicy(Code, Label, UpdatedAt, Rating, LeaveDays, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
