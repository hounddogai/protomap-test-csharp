namespace Acme.Workforce.Talent.SkillProfile.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile agreement is in its life cycle.</summary>
public enum SkillProfileAgreementStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One skill profile agreement, with what the planners need to act on it.</summary>
public sealed class SkillProfileAgreement(string code, string title, int quantity, bool approved, string label, int retries, int attempts, int version, SkillProfileAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public bool Approved { get; set; } = approved;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public int Attempts { get; set; } = attempts;
    public int Version { get; set; } = version;
    public SkillProfileAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile agreement in status idle.</summary>
    public static SkillProfileAgreement NewSkillProfileAgreement(string code, string title)
    {
        return new SkillProfileAgreement(code, title, 0, false, "", 0, 0, 0, SkillProfileAgreementStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileAgreementStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileAgreementStatus next)
    {
        switch (Status)
        {
            case SkillProfileAgreementStatus.Idle:
                return next == SkillProfileAgreementStatus.Warming || next == SkillProfileAgreementStatus.Ready;
            case SkillProfileAgreementStatus.Warming:
                return next == SkillProfileAgreementStatus.Ready || next == SkillProfileAgreementStatus.Draining;
            case SkillProfileAgreementStatus.Ready:
                switch (next)
                {
                    case SkillProfileAgreementStatus.Draining:
                    case SkillProfileAgreementStatus.Stopped:
                        return true;
                }
                return false;
            case SkillProfileAgreementStatus.Draining:
                switch (next)
                {
                    case SkillProfileAgreementStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SkillProfileAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Title}]";
    }

    public double NetTotal()
    {
        return Math.Round((double)Attempts + (double)Quantity, 2);
    }

    public SkillProfileAgreement Copy()
    {
        return new SkillProfileAgreement(Code, Title, Quantity, Approved, Label, Retries, Attempts, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
