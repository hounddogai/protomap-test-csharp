namespace Acme.Workforce.Talent.SkillProfile.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile forecast is in its life cycle.</summary>
public enum SkillProfileForecastStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a skill profile forecast and its current status.</summary>
public sealed class SkillProfileForecast(string code, string name, Dictionary<string, string> attributes, int leaveDays, int startsAt, SkillProfileForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int LeaveDays { get; set; } = leaveDays;
    public int StartsAt { get; set; } = startsAt;
    public SkillProfileForecastStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile forecast in status open.</summary>
    public static SkillProfileForecast NewSkillProfileForecast(string code, string name)
    {
        return new SkillProfileForecast(
            code,
            name,
            new Dictionary<string, string>(),
            0,
            0,
            SkillProfileForecastStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileForecastStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileForecastStatus next)
    {
        switch (Status)
        {
            case SkillProfileForecastStatus.Open:
                return next == SkillProfileForecastStatus.Resolved || next == SkillProfileForecastStatus.Reopened;
            case SkillProfileForecastStatus.InReview:
                return next == SkillProfileForecastStatus.Archived;
            case SkillProfileForecastStatus.Resolved:
                switch (next)
                {
                    case SkillProfileForecastStatus.Archived:
                        return true;
                }
                return false;
            case SkillProfileForecastStatus.Reopened:
                switch (next)
                {
                    case SkillProfileForecastStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SkillProfileForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double EffectiveValue()
    {
        return Math.Round((double)StartsAt * (double)LeaveDays, 2);
    }

    public SkillProfileForecast Copy()
    {
        return new SkillProfileForecast(Code, Name, Attributes, LeaveDays, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
