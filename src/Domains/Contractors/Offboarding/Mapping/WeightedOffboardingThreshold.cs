namespace Acme.Workforce.Contractors.Offboarding.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted offboarding threshold is in its life cycle.</summary>
public enum WeightedOffboardingThresholdStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a weighted offboarding threshold and its current status.</summary>
public sealed class WeightedOffboardingThreshold(string code, string label, string contactEmail, string unit, string checksum, int leaveDays, bool visible, int version, WeightedOffboardingThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ContactEmail { get; set; } = contactEmail;
    public string Unit { get; set; } = unit;
    public string Checksum { get; set; } = checksum;
    public int LeaveDays { get; set; } = leaveDays;
    public bool Visible { get; set; } = visible;
    public int Version { get; set; } = version;
    public WeightedOffboardingThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a weighted offboarding threshold in status draft.</summary>
    public static WeightedOffboardingThreshold NewWeightedOffboardingThreshold(string code, string label)
    {
        return new WeightedOffboardingThreshold(
            code,
            label,
            "",
            "",
            "",
            0,
            false,
            0,
            WeightedOffboardingThresholdStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedOffboardingThresholdStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedOffboardingThresholdStatus next)
    {
        switch (Status)
        {
            case WeightedOffboardingThresholdStatus.Draft:
                switch (next)
                {
                    case WeightedOffboardingThresholdStatus.Active:
                    case WeightedOffboardingThresholdStatus.Closed:
                        return true;
                }
                return false;
            case WeightedOffboardingThresholdStatus.Pending:
                return next == WeightedOffboardingThresholdStatus.Active;
            case WeightedOffboardingThresholdStatus.Active:
                return next == WeightedOffboardingThresholdStatus.Suspended;
            case WeightedOffboardingThresholdStatus.Suspended:
                switch (next)
                {
                    case WeightedOffboardingThresholdStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedOffboardingThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted offboarding threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Version - (double)LeaveDays, 2);
    }

    public WeightedOffboardingThreshold Copy()
    {
        return new WeightedOffboardingThreshold(
            Code,
            Label,
            ContactEmail,
            Unit,
            Checksum,
            LeaveDays,
            Visible,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
