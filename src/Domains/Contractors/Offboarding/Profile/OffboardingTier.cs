namespace Acme.Workforce.Contractors.Offboarding.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding tier is in its life cycle.</summary>
public enum OffboardingTierStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A offboarding tier. Its code is unique within a tenant.</summary>
public sealed class OffboardingTier(string code, string label, int windowDays, string skill, double threshold, string timezone, double minimum, double margin, double amount, OffboardingTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public string Skill { get; set; } = skill;
    public double Threshold { get; set; } = threshold;
    public string Timezone { get; set; } = timezone;
    public double Minimum { get; set; } = minimum;
    public double Margin { get; set; } = margin;
    public double Amount { get; set; } = amount;
    public OffboardingTierStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding tier in status idle.</summary>
    public static OffboardingTier NewOffboardingTier(string code, string label)
    {
        return new OffboardingTier(code, label, 0, "", 0.0, "", 0.0, 0.0, 0.0, OffboardingTierStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingTierStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingTierStatus next)
    {
        switch (Status)
        {
            case OffboardingTierStatus.Idle:
                switch (next)
                {
                    case OffboardingTierStatus.Warming:
                        return true;
                }
                return false;
            case OffboardingTierStatus.Warming:
                switch (next)
                {
                    case OffboardingTierStatus.Stopped:
                        return true;
                }
                return false;
            case OffboardingTierStatus.Ready:
                return next == OffboardingTierStatus.Draining;
            case OffboardingTierStatus.Draining:
                return next == OffboardingTierStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(OffboardingTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"offboarding tier"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)WindowDays * Minimum, 2);
    }

    public OffboardingTier Copy()
    {
        return new OffboardingTier(
            Code,
            Label,
            WindowDays,
            Skill,
            Threshold,
            Timezone,
            Minimum,
            Margin,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
