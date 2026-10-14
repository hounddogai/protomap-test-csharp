namespace Acme.Workforce.Contractors.Offboarding.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding preference is in its life cycle.</summary>
public enum OffboardingPreferenceStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One offboarding preference, with what the planners need to act on it.</summary>
public sealed class OffboardingPreference(string code, string label, string team, int windowDays, string location, double floorValue, string salaryBand, int updatedAt, int retries, OffboardingPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public int WindowDays { get; set; } = windowDays;
    public string Location { get; set; } = location;
    public double FloorValue { get; set; } = floorValue;
    public string SalaryBand { get; set; } = salaryBand;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Retries { get; set; } = retries;
    public OffboardingPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding preference in status idle.</summary>
    public static OffboardingPreference NewOffboardingPreference(string code, string label)
    {
        return new OffboardingPreference(code, label, "", 0, "", 0.0, "", 0, 0, OffboardingPreferenceStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingPreferenceStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingPreferenceStatus next)
    {
        switch (Status)
        {
            case OffboardingPreferenceStatus.Idle:
                return next == OffboardingPreferenceStatus.Ready || next == OffboardingPreferenceStatus.Stopped;
            case OffboardingPreferenceStatus.Warming:
                switch (next)
                {
                    case OffboardingPreferenceStatus.Ready:
                    case OffboardingPreferenceStatus.Stopped:
                        return true;
                }
                return false;
            case OffboardingPreferenceStatus.Ready:
                switch (next)
                {
                    case OffboardingPreferenceStatus.Draining:
                    case OffboardingPreferenceStatus.Stopped:
                        return true;
                }
                return false;
            case OffboardingPreferenceStatus.Draining:
                switch (next)
                {
                    case OffboardingPreferenceStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OffboardingPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(FloorValue + (double)WindowDays, 2);
    }

    public OffboardingPreference Copy()
    {
        return new OffboardingPreference(
            Code,
            Label,
            Team,
            WindowDays,
            Location,
            FloorValue,
            SalaryBand,
            UpdatedAt,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
