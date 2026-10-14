namespace Acme.Workforce.Contractors.Offboarding.OffboardingSlot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy offboarding forecast is in its life cycle.</summary>
public enum LegacyOffboardingForecastStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One legacy offboarding forecast, with what the planners need to act on it.</summary>
public sealed class LegacyOffboardingForecast(string code, string label, int windowDays, string channel, string description, string jobCode, string salaryBand, bool pinned, string locale, int durationMinutes, double minimum, LegacyOffboardingForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public string Channel { get; set; } = channel;
    public string Description { get; set; } = description;
    public string JobCode { get; set; } = jobCode;
    public string SalaryBand { get; set; } = salaryBand;
    public bool Pinned { get; set; } = pinned;
    public string Locale { get; set; } = locale;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Minimum { get; set; } = minimum;
    public LegacyOffboardingForecastStatus Status { get; set; } = status;

    /// <summary>Starts a legacy offboarding forecast in status proposed.</summary>
    public static LegacyOffboardingForecast NewLegacyOffboardingForecast(string code, string label)
    {
        return new LegacyOffboardingForecast(
            code,
            label,
            0,
            "",
            "",
            "",
            "",
            false,
            "",
            0,
            0.0,
            LegacyOffboardingForecastStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyOffboardingForecastStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyOffboardingForecastStatus next)
    {
        switch (Status)
        {
            case LegacyOffboardingForecastStatus.Proposed:
                return next == LegacyOffboardingForecastStatus.Approved || next == LegacyOffboardingForecastStatus.Rejected;
            case LegacyOffboardingForecastStatus.Approved:
                switch (next)
                {
                    case LegacyOffboardingForecastStatus.Rejected:
                        return true;
                }
                return false;
            case LegacyOffboardingForecastStatus.Rejected:
                return next == LegacyOffboardingForecastStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LegacyOffboardingForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy offboarding forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)DurationMinutes - (double)WindowDays, 2);
    }

    public LegacyOffboardingForecast Copy()
    {
        return new LegacyOffboardingForecast(
            Code,
            Label,
            WindowDays,
            Channel,
            Description,
            JobCode,
            SalaryBand,
            Pinned,
            Locale,
            DurationMinutes,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
