namespace Acme.Workforce.Contractors.Rate.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual rate override is in its life cycle.</summary>
public enum ManualRateOverrideStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A manual rate override as the rate team keeps it.</summary>
public sealed class ManualRateOverride(string code, string name, bool approved, string externalRef, int sequence, double ratio, string locale, int graceDays, double score, ManualRateOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Approved { get; set; } = approved;
    public string ExternalRef { get; set; } = externalRef;
    public int Sequence { get; set; } = sequence;
    public double Ratio { get; set; } = ratio;
    public string Locale { get; set; } = locale;
    public int GraceDays { get; set; } = graceDays;
    public double Score { get; set; } = score;
    public ManualRateOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a manual rate override in status created.</summary>
    public static ManualRateOverride NewManualRateOverride(string code, string name)
    {
        return new ManualRateOverride(code, name, false, "", 0, 0.0, "", 0, 0.0, ManualRateOverrideStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualRateOverrideStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualRateOverrideStatus next)
    {
        switch (Status)
        {
            case ManualRateOverrideStatus.Created:
                switch (next)
                {
                    case ManualRateOverrideStatus.Retired:
                        return true;
                }
                return false;
            case ManualRateOverrideStatus.Validated:
                return next == ManualRateOverrideStatus.Published || next == ManualRateOverrideStatus.Retired;
            case ManualRateOverrideStatus.Published:
                switch (next)
                {
                    case ManualRateOverrideStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualRateOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual rate override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Score - (double)GraceDays, 2);
    }

    public ManualRateOverride Copy()
    {
        return new ManualRateOverride(
            Code,
            Name,
            Approved,
            ExternalRef,
            Sequence,
            Ratio,
            Locale,
            GraceDays,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
