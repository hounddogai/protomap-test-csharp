namespace Acme.Workforce.Contractors.Rate.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate override is in its life cycle.</summary>
public enum RateOverrideStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One rate override, with what the planners need to act on it.</summary>
public sealed class RateOverride(string code, string name, double score, string description, string locale, double hours, string source, int attempts, bool pinned, RateOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public string Description { get; set; } = description;
    public string Locale { get; set; } = locale;
    public double Hours { get; set; } = hours;
    public string Source { get; set; } = source;
    public int Attempts { get; set; } = attempts;
    public bool Pinned { get; set; } = pinned;
    public RateOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a rate override in status proposed.</summary>
    public static RateOverride NewRateOverride(string code, string name)
    {
        return new RateOverride(code, name, 0.0, "", "", 0.0, "", 0, false, RateOverrideStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateOverrideStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateOverrideStatus next)
    {
        switch (Status)
        {
            case RateOverrideStatus.Proposed:
                return next == RateOverrideStatus.Approved || next == RateOverrideStatus.Rejected;
            case RateOverrideStatus.Approved:
                switch (next)
                {
                    case RateOverrideStatus.Rejected:
                    case RateOverrideStatus.Withdrawn:
                        return true;
                }
                return false;
            case RateOverrideStatus.Rejected:
                return next == RateOverrideStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RateOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rate override"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Hours * (double)Attempts, 2);
    }

    public RateOverride Copy()
    {
        return new RateOverride(Code, Name, Score, Description, Locale, Hours, Source, Attempts, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
