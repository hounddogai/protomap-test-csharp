namespace Acme.Workforce.Talent.SkillProfile.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile preference is in its life cycle.</summary>
public enum SkillProfilePreferenceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a skill profile preference and its current status.</summary>
public sealed class SkillProfilePreference(string code, string label, string country, double margin, string managerId, string region, int batchSize, string department, double threshold, SkillProfilePreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public double Margin { get; set; } = margin;
    public string ManagerId { get; set; } = managerId;
    public string Region { get; set; } = region;
    public int BatchSize { get; set; } = batchSize;
    public string Department { get; set; } = department;
    public double Threshold { get; set; } = threshold;
    public SkillProfilePreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile preference in status created.</summary>
    public static SkillProfilePreference NewSkillProfilePreference(string code, string label)
    {
        return new SkillProfilePreference(
            code,
            label,
            "",
            0.0,
            "",
            "",
            0,
            "",
            0.0,
            SkillProfilePreferenceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfilePreferenceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfilePreferenceStatus next)
    {
        switch (Status)
        {
            case SkillProfilePreferenceStatus.Created:
                switch (next)
                {
                    case SkillProfilePreferenceStatus.Published:
                        return true;
                }
                return false;
            case SkillProfilePreferenceStatus.Validated:
                return next == SkillProfilePreferenceStatus.Published || next == SkillProfilePreferenceStatus.Retired;
            case SkillProfilePreferenceStatus.Published:
                switch (next)
                {
                    case SkillProfilePreferenceStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SkillProfilePreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"skill profile preference"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)BatchSize * Threshold, 2);
    }

    public SkillProfilePreference Copy()
    {
        return new SkillProfilePreference(
            Code,
            Label,
            Country,
            Margin,
            ManagerId,
            Region,
            BatchSize,
            Department,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
