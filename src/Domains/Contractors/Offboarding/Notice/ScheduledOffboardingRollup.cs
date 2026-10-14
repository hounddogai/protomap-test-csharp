namespace Acme.Workforce.Contractors.Offboarding.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled offboarding rollup is in its life cycle.</summary>
public enum ScheduledOffboardingRollupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One scheduled offboarding rollup, with what the planners need to act on it.</summary>
public sealed class ScheduledOffboardingRollup(string code, string title, string team, string locale, string country, string region, double amount, Dictionary<string, string> attributes, int revision, int endsAt, ScheduledOffboardingRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public string Locale { get; set; } = locale;
    public string Country { get; set; } = country;
    public string Region { get; set; } = region;
    public double Amount { get; set; } = amount;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Revision { get; set; } = revision;
    public int EndsAt { get; set; } = endsAt;
    public ScheduledOffboardingRollupStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled offboarding rollup in status proposed.</summary>
    public static ScheduledOffboardingRollup NewScheduledOffboardingRollup(string code, string title)
    {
        return new ScheduledOffboardingRollup(
            code,
            title,
            "",
            "",
            "",
            "",
            0.0,
            new Dictionary<string, string>(),
            0,
            0,
            ScheduledOffboardingRollupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledOffboardingRollupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledOffboardingRollupStatus next)
    {
        switch (Status)
        {
            case ScheduledOffboardingRollupStatus.Proposed:
                switch (next)
                {
                    case ScheduledOffboardingRollupStatus.Rejected:
                        return true;
                }
                return false;
            case ScheduledOffboardingRollupStatus.Approved:
                return next == ScheduledOffboardingRollupStatus.Withdrawn;
            case ScheduledOffboardingRollupStatus.Rejected:
                switch (next)
                {
                    case ScheduledOffboardingRollupStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledOffboardingRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled offboarding rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
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

    public double NetLoad()
    {
        return Math.Round((double)Revision + Amount, 2);
    }

    public ScheduledOffboardingRollup Copy()
    {
        return new ScheduledOffboardingRollup(
            Code,
            Title,
            Team,
            Locale,
            Country,
            Region,
            Amount,
            Attributes,
            Revision,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
