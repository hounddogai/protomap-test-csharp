namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default pulse survey override is in its life cycle.</summary>
public enum DefaultPulseSurveyOverrideStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a default pulse survey override and its current status.</summary>
public sealed class DefaultPulseSurveyOverride(string code, string name, string locale, string externalRef, string salaryBand, int createdAt, int startsAt, int endsAt, int capacity, string category, DefaultPulseSurveyOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public string ExternalRef { get; set; } = externalRef;
    public string SalaryBand { get; set; } = salaryBand;
    public int CreatedAt { get; set; } = createdAt;
    public int StartsAt { get; set; } = startsAt;
    public int EndsAt { get; set; } = endsAt;
    public int Capacity { get; set; } = capacity;
    public string Category { get; set; } = category;
    public DefaultPulseSurveyOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a default pulse survey override in status new.</summary>
    public static DefaultPulseSurveyOverride NewDefaultPulseSurveyOverride(string code, string name)
    {
        return new DefaultPulseSurveyOverride(
            code,
            name,
            "",
            "",
            "",
            0,
            0,
            0,
            0,
            "",
            DefaultPulseSurveyOverrideStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPulseSurveyOverrideStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPulseSurveyOverrideStatus next)
    {
        switch (Status)
        {
            case DefaultPulseSurveyOverrideStatus.New:
                return next == DefaultPulseSurveyOverrideStatus.Running;
            case DefaultPulseSurveyOverrideStatus.Queued:
                return next == DefaultPulseSurveyOverrideStatus.Running || next == DefaultPulseSurveyOverrideStatus.Done;
            case DefaultPulseSurveyOverrideStatus.Running:
                switch (next)
                {
                    case DefaultPulseSurveyOverrideStatus.Done:
                        return true;
                }
                return false;
            case DefaultPulseSurveyOverrideStatus.Done:
                return next == DefaultPulseSurveyOverrideStatus.Failed;
        }
        return false;
    }

    public void MoveTo(DefaultPulseSurveyOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default pulse survey override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetExposure()
    {
        return Math.Round((double)CreatedAt + (double)StartsAt, 2);
    }

    public DefaultPulseSurveyOverride Copy()
    {
        return new DefaultPulseSurveyOverride(
            Code,
            Name,
            Locale,
            ExternalRef,
            SalaryBand,
            CreatedAt,
            StartsAt,
            EndsAt,
            Capacity,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
