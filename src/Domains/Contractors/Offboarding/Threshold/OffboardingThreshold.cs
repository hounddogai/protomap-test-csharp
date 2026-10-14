namespace Acme.Workforce.Contractors.Offboarding.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding threshold is in its life cycle.</summary>
public enum OffboardingThresholdStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A offboarding threshold as the offboarding team keeps it.</summary>
public sealed class OffboardingThreshold(string code, string title, double weight, bool active, string skill, double hours, int durationMinutes, int quantity, OffboardingThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public bool Active { get; set; } = active;
    public string Skill { get; set; } = skill;
    public double Hours { get; set; } = hours;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Quantity { get; set; } = quantity;
    public OffboardingThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding threshold in status requested.</summary>
    public static OffboardingThreshold NewOffboardingThreshold(string code, string title)
    {
        return new OffboardingThreshold(
            code,
            title,
            0.0,
            false,
            "",
            0.0,
            0,
            0,
            OffboardingThresholdStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingThresholdStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingThresholdStatus next)
    {
        switch (Status)
        {
            case OffboardingThresholdStatus.Requested:
                switch (next)
                {
                    case OffboardingThresholdStatus.Revoked:
                    case OffboardingThresholdStatus.Expired:
                        return true;
                }
                return false;
            case OffboardingThresholdStatus.Granted:
                switch (next)
                {
                    case OffboardingThresholdStatus.Expired:
                        return true;
                }
                return false;
            case OffboardingThresholdStatus.Revoked:
                switch (next)
                {
                    case OffboardingThresholdStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OffboardingThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Quantity - (double)DurationMinutes, 2);
    }

    public OffboardingThreshold Copy()
    {
        return new OffboardingThreshold(
            Code,
            Title,
            Weight,
            Active,
            Skill,
            Hours,
            DurationMinutes,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
