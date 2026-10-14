namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey record is in its life cycle.</summary>
public enum PulseSurveyRecordStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One pulse survey record, with what the planners need to act on it.</summary>
public sealed class PulseSurveyRecord(string code, string name, string description, string locale, int leaveDays, int tenureMonths, PulseSurveyRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public string Locale { get; set; } = locale;
    public int LeaveDays { get; set; } = leaveDays;
    public int TenureMonths { get; set; } = tenureMonths;
    public PulseSurveyRecordStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey record in status idle.</summary>
    public static PulseSurveyRecord NewPulseSurveyRecord(string code, string name)
    {
        return new PulseSurveyRecord(code, name, "", "", 0, 0, PulseSurveyRecordStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyRecordStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyRecordStatus next)
    {
        switch (Status)
        {
            case PulseSurveyRecordStatus.Idle:
                return next == PulseSurveyRecordStatus.Stopped;
            case PulseSurveyRecordStatus.Warming:
                return next == PulseSurveyRecordStatus.Draining;
            case PulseSurveyRecordStatus.Ready:
                switch (next)
                {
                    case PulseSurveyRecordStatus.Draining:
                        return true;
                }
                return false;
            case PulseSurveyRecordStatus.Draining:
                return next == PulseSurveyRecordStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PulseSurveyRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetExposure()
    {
        return Math.Round((double)LeaveDays - (double)TenureMonths, 2);
    }

    public PulseSurveyRecord Copy()
    {
        return new PulseSurveyRecord(Code, Name, Description, Locale, LeaveDays, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
