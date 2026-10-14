namespace Acme.Workforce.Talent.SkillProfile.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy skill profile preference is in its life cycle.</summary>
public enum LegacySkillProfilePreferenceStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A legacy skill profile preference. Its code is unique within a tenant.</summary>
public sealed class LegacySkillProfilePreference(string code, string name, string locale, int endsAt, string summary, LegacySkillProfilePreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public int EndsAt { get; set; } = endsAt;
    public string Summary { get; set; } = summary;
    public LegacySkillProfilePreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a legacy skill profile preference in status requested.</summary>
    public static LegacySkillProfilePreference NewLegacySkillProfilePreference(string code, string name)
    {
        return new LegacySkillProfilePreference(code, name, "", 0, "", LegacySkillProfilePreferenceStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacySkillProfilePreferenceStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacySkillProfilePreferenceStatus next)
    {
        switch (Status)
        {
            case LegacySkillProfilePreferenceStatus.Requested:
                switch (next)
                {
                    case LegacySkillProfilePreferenceStatus.Granted:
                        return true;
                }
                return false;
            case LegacySkillProfilePreferenceStatus.Granted:
                switch (next)
                {
                    case LegacySkillProfilePreferenceStatus.Revoked:
                    case LegacySkillProfilePreferenceStatus.Expired:
                        return true;
                }
                return false;
            case LegacySkillProfilePreferenceStatus.Revoked:
                return next == LegacySkillProfilePreferenceStatus.Expired;
        }
        return false;
    }

    public void MoveTo(LegacySkillProfilePreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy skill profile preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return (double)EndsAt;
    }

    public LegacySkillProfilePreference Copy()
    {
        return new LegacySkillProfilePreference(Code, Name, Locale, EndsAt, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
