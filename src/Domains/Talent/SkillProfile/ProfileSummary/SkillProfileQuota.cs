namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile quota is in its life cycle.</summary>
public enum SkillProfileQuotaStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A skill profile quota. Its code is unique within a tenant.</summary>
public sealed class SkillProfileQuota(string code, string label, string checksum, double fte, string badge, string skill, double amount, string currency, SkillProfileQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public double Fte { get; set; } = fte;
    public string Badge { get; set; } = badge;
    public string Skill { get; set; } = skill;
    public double Amount { get; set; } = amount;
    public string Currency { get; set; } = currency;
    public SkillProfileQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile quota in status planned.</summary>
    public static SkillProfileQuota NewSkillProfileQuota(string code, string label)
    {
        return new SkillProfileQuota(code, label, "", 0.0, "", "", 0.0, "", SkillProfileQuotaStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileQuotaStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileQuotaStatus next)
    {
        switch (Status)
        {
            case SkillProfileQuotaStatus.Planned:
                switch (next)
                {
                    case SkillProfileQuotaStatus.Scheduled:
                    case SkillProfileQuotaStatus.Completed:
                        return true;
                }
                return false;
            case SkillProfileQuotaStatus.Scheduled:
                return next == SkillProfileQuotaStatus.Completed;
            case SkillProfileQuotaStatus.InProgress:
                switch (next)
                {
                    case SkillProfileQuotaStatus.Completed:
                        return true;
                }
                return false;
            case SkillProfileQuotaStatus.Completed:
                return next == SkillProfileQuotaStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SkillProfileQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Fte + Amount, 2);
    }

    public SkillProfileQuota Copy()
    {
        return new SkillProfileQuota(Code, Label, Checksum, Fte, Badge, Skill, Amount, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
