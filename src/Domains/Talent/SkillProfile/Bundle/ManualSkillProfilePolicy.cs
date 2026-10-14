namespace Acme.Workforce.Talent.SkillProfile.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual skill profile policy is in its life cycle.</summary>
public enum ManualSkillProfilePolicyStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One manual skill profile policy, with what the planners need to act on it.</summary>
public sealed class ManualSkillProfilePolicy(string code, string title, bool pinned, int updatedAt, string grade, bool archived, string managerId, string jobCode, string externalRef, int tenureMonths, string reviewer, ManualSkillProfilePolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Grade { get; set; } = grade;
    public bool Archived { get; set; } = archived;
    public string ManagerId { get; set; } = managerId;
    public string JobCode { get; set; } = jobCode;
    public string ExternalRef { get; set; } = externalRef;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Reviewer { get; set; } = reviewer;
    public ManualSkillProfilePolicyStatus Status { get; set; } = status;

    /// <summary>Starts a manual skill profile policy in status requested.</summary>
    public static ManualSkillProfilePolicy NewManualSkillProfilePolicy(string code, string title)
    {
        return new ManualSkillProfilePolicy(
            code,
            title,
            false,
            0,
            "",
            false,
            "",
            "",
            "",
            0,
            "",
            ManualSkillProfilePolicyStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualSkillProfilePolicyStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualSkillProfilePolicyStatus next)
    {
        switch (Status)
        {
            case ManualSkillProfilePolicyStatus.Requested:
                return next == ManualSkillProfilePolicyStatus.Revoked || next == ManualSkillProfilePolicyStatus.Expired;
            case ManualSkillProfilePolicyStatus.Granted:
                return next == ManualSkillProfilePolicyStatus.Revoked;
            case ManualSkillProfilePolicyStatus.Revoked:
                return next == ManualSkillProfilePolicyStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ManualSkillProfilePolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual skill profile policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)TenureMonths * (double)UpdatedAt, 2);
    }

    public ManualSkillProfilePolicy Copy()
    {
        return new ManualSkillProfilePolicy(
            Code,
            Title,
            Pinned,
            UpdatedAt,
            Grade,
            Archived,
            ManagerId,
            JobCode,
            ExternalRef,
            TenureMonths,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
