namespace Acme.Workforce.Contractors.Offboarding.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding request is in its life cycle.</summary>
public enum OffboardingRequestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A offboarding request. Its code is unique within a tenant.</summary>
public sealed class OffboardingRequest(string code, string title, double budget, string grade, double ceiling, int durationMinutes, string locale, string skill, OffboardingRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Budget { get; set; } = budget;
    public string Grade { get; set; } = grade;
    public double Ceiling { get; set; } = ceiling;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Locale { get; set; } = locale;
    public string Skill { get; set; } = skill;
    public OffboardingRequestStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding request in status planned.</summary>
    public static OffboardingRequest NewOffboardingRequest(string code, string title)
    {
        return new OffboardingRequest(code, title, 0.0, "", 0.0, 0, "", "", OffboardingRequestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingRequestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingRequestStatus next)
    {
        switch (Status)
        {
            case OffboardingRequestStatus.Planned:
                return next == OffboardingRequestStatus.Cancelled;
            case OffboardingRequestStatus.Scheduled:
                switch (next)
                {
                    case OffboardingRequestStatus.InProgress:
                        return true;
                }
                return false;
            case OffboardingRequestStatus.InProgress:
                switch (next)
                {
                    case OffboardingRequestStatus.Cancelled:
                        return true;
                }
                return false;
            case OffboardingRequestStatus.Completed:
                switch (next)
                {
                    case OffboardingRequestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OffboardingRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"offboarding request"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)DurationMinutes + Ceiling, 2);
    }

    public OffboardingRequest Copy()
    {
        return new OffboardingRequest(Code, Title, Budget, Grade, Ceiling, DurationMinutes, Locale, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
