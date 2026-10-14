namespace Acme.Workforce.Talent.SkillProfile.ProfileGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal skill profile preference is in its life cycle.</summary>
public enum SeasonalSkillProfilePreferenceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A seasonal skill profile preference as the skill profile team keeps it.</summary>
public sealed class SeasonalSkillProfilePreference(string code, string title, bool pinned, int tenureMonths, Dictionary<string, string> attributes, double budget, string locale, string country, double rating, double score, SeasonalSkillProfilePreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public int TenureMonths { get; set; } = tenureMonths;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Budget { get; set; } = budget;
    public string Locale { get; set; } = locale;
    public string Country { get; set; } = country;
    public double Rating { get; set; } = rating;
    public double Score { get; set; } = score;
    public SeasonalSkillProfilePreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal skill profile preference in status planned.</summary>
    public static SeasonalSkillProfilePreference NewSeasonalSkillProfilePreference(string code, string title)
    {
        return new SeasonalSkillProfilePreference(
            code,
            title,
            false,
            0,
            new Dictionary<string, string>(),
            0.0,
            "",
            "",
            0.0,
            0.0,
            SeasonalSkillProfilePreferenceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalSkillProfilePreferenceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalSkillProfilePreferenceStatus next)
    {
        switch (Status)
        {
            case SeasonalSkillProfilePreferenceStatus.Planned:
                return next == SeasonalSkillProfilePreferenceStatus.Cancelled;
            case SeasonalSkillProfilePreferenceStatus.Scheduled:
                return next == SeasonalSkillProfilePreferenceStatus.Cancelled;
            case SeasonalSkillProfilePreferenceStatus.InProgress:
                switch (next)
                {
                    case SeasonalSkillProfilePreferenceStatus.Completed:
                        return true;
                }
                return false;
            case SeasonalSkillProfilePreferenceStatus.Completed:
                switch (next)
                {
                    case SeasonalSkillProfilePreferenceStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalSkillProfilePreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal skill profile preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double EffectiveLoad()
    {
        return Math.Round(Score * Rating, 2);
    }

    public SeasonalSkillProfilePreference Copy()
    {
        return new SeasonalSkillProfilePreference(
            Code,
            Title,
            Pinned,
            TenureMonths,
            Attributes,
            Budget,
            Locale,
            Country,
            Rating,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
