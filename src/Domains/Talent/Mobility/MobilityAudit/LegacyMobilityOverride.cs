namespace Acme.Workforce.Talent.Mobility.MobilityAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy mobility override is in its life cycle.</summary>
public enum LegacyMobilityOverrideStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A legacy mobility override as the mobility team keeps it.</summary>
public sealed class LegacyMobilityOverride(string code, string label, double rate, int retries, double threshold, double ceiling, LegacyMobilityOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public int Retries { get; set; } = retries;
    public double Threshold { get; set; } = threshold;
    public double Ceiling { get; set; } = ceiling;
    public LegacyMobilityOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a legacy mobility override in status draft.</summary>
    public static LegacyMobilityOverride NewLegacyMobilityOverride(string code, string label)
    {
        return new LegacyMobilityOverride(code, label, 0.0, 0, 0.0, 0.0, LegacyMobilityOverrideStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyMobilityOverrideStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyMobilityOverrideStatus next)
    {
        switch (Status)
        {
            case LegacyMobilityOverrideStatus.Draft:
                return next == LegacyMobilityOverrideStatus.Pending || next == LegacyMobilityOverrideStatus.Suspended;
            case LegacyMobilityOverrideStatus.Pending:
                switch (next)
                {
                    case LegacyMobilityOverrideStatus.Suspended:
                    case LegacyMobilityOverrideStatus.Closed:
                        return true;
                }
                return false;
            case LegacyMobilityOverrideStatus.Active:
                switch (next)
                {
                    case LegacyMobilityOverrideStatus.Suspended:
                        return true;
                }
                return false;
            case LegacyMobilityOverrideStatus.Suspended:
                switch (next)
                {
                    case LegacyMobilityOverrideStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyMobilityOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy mobility override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Threshold * Rate, 2);
    }

    public LegacyMobilityOverride Copy()
    {
        return new LegacyMobilityOverride(Code, Label, Rate, Retries, Threshold, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
