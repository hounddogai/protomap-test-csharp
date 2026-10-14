namespace Acme.Workforce.Contractors.Offboarding.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient offboarding forecast is in its life cycle.</summary>
public enum LenientOffboardingForecastStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One lenient offboarding forecast, with what the planners need to act on it.</summary>
public sealed class LenientOffboardingForecast(string code, string name, string category, double threshold, string locale, string managerId, LenientOffboardingForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public double Threshold { get; set; } = threshold;
    public string Locale { get; set; } = locale;
    public string ManagerId { get; set; } = managerId;
    public LenientOffboardingForecastStatus Status { get; set; } = status;

    /// <summary>Starts a lenient offboarding forecast in status proposed.</summary>
    public static LenientOffboardingForecast NewLenientOffboardingForecast(string code, string name)
    {
        return new LenientOffboardingForecast(
            code,
            name,
            "",
            0.0,
            "",
            "",
            LenientOffboardingForecastStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientOffboardingForecastStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientOffboardingForecastStatus next)
    {
        switch (Status)
        {
            case LenientOffboardingForecastStatus.Proposed:
                return next == LenientOffboardingForecastStatus.Rejected || next == LenientOffboardingForecastStatus.Withdrawn;
            case LenientOffboardingForecastStatus.Approved:
                switch (next)
                {
                    case LenientOffboardingForecastStatus.Withdrawn:
                        return true;
                }
                return false;
            case LenientOffboardingForecastStatus.Rejected:
                return next == LenientOffboardingForecastStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LenientOffboardingForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient offboarding forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return Threshold;
    }

    public LenientOffboardingForecast Copy()
    {
        return new LenientOffboardingForecast(Code, Name, Category, Threshold, Locale, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
