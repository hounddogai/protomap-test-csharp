namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict pulse survey preference is in its life cycle.</summary>
public enum StrictPulseSurveyPreferenceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A strict pulse survey preference as the pulse survey team keeps it.</summary>
public sealed class StrictPulseSurveyPreference(string code, string name, int windowDays, string grade, bool active, string country, string category, StrictPulseSurveyPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public string Grade { get; set; } = grade;
    public bool Active { get; set; } = active;
    public string Country { get; set; } = country;
    public string Category { get; set; } = category;
    public StrictPulseSurveyPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a strict pulse survey preference in status new.</summary>
    public static StrictPulseSurveyPreference NewStrictPulseSurveyPreference(string code, string name)
    {
        return new StrictPulseSurveyPreference(
            code,
            name,
            0,
            "",
            false,
            "",
            "",
            StrictPulseSurveyPreferenceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPulseSurveyPreferenceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPulseSurveyPreferenceStatus next)
    {
        switch (Status)
        {
            case StrictPulseSurveyPreferenceStatus.New:
                return next == StrictPulseSurveyPreferenceStatus.Running || next == StrictPulseSurveyPreferenceStatus.Done;
            case StrictPulseSurveyPreferenceStatus.Queued:
                return next == StrictPulseSurveyPreferenceStatus.Running || next == StrictPulseSurveyPreferenceStatus.Done;
            case StrictPulseSurveyPreferenceStatus.Running:
                switch (next)
                {
                    case StrictPulseSurveyPreferenceStatus.Done:
                        return true;
                }
                return false;
            case StrictPulseSurveyPreferenceStatus.Done:
                switch (next)
                {
                    case StrictPulseSurveyPreferenceStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictPulseSurveyPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict pulse survey preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public StrictPulseSurveyPreference Copy()
    {
        return new StrictPulseSurveyPreference(Code, Name, WindowDays, Grade, Active, Country, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
