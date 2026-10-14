namespace Acme.Workforce.Talent.SkillProfile.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile bundle is in its life cycle.</summary>
public enum SkillProfileBundleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One skill profile bundle, with what the planners need to act on it.</summary>
public sealed class SkillProfileBundle(string code, string label, string timezone, string externalRef, double margin, SkillProfileBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Timezone { get; set; } = timezone;
    public string ExternalRef { get; set; } = externalRef;
    public double Margin { get; set; } = margin;
    public SkillProfileBundleStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile bundle in status created.</summary>
    public static SkillProfileBundle NewSkillProfileBundle(string code, string label)
    {
        return new SkillProfileBundle(code, label, "", "", 0.0, SkillProfileBundleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileBundleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileBundleStatus next)
    {
        switch (Status)
        {
            case SkillProfileBundleStatus.Created:
                return next == SkillProfileBundleStatus.Validated || next == SkillProfileBundleStatus.Retired;
            case SkillProfileBundleStatus.Validated:
                return next == SkillProfileBundleStatus.Published || next == SkillProfileBundleStatus.Retired;
            case SkillProfileBundleStatus.Published:
                return next == SkillProfileBundleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(SkillProfileBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Margin;
    }

    public SkillProfileBundle Copy()
    {
        return new SkillProfileBundle(Code, Label, Timezone, ExternalRef, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
