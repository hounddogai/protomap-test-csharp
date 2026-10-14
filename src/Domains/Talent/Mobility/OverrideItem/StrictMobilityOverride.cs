namespace Acme.Workforce.Talent.Mobility.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict mobility override is in its life cycle.</summary>
public enum StrictMobilityOverrideStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One strict mobility override, with what the planners need to act on it.</summary>
public sealed class StrictMobilityOverride(string code, string title, string currency, int priority, string label, string segment, int durationMinutes, double ceiling, double maximum, int revision, StrictMobilityOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public int Priority { get; set; } = priority;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Ceiling { get; set; } = ceiling;
    public double Maximum { get; set; } = maximum;
    public int Revision { get; set; } = revision;
    public StrictMobilityOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a strict mobility override in status requested.</summary>
    public static StrictMobilityOverride NewStrictMobilityOverride(string code, string title)
    {
        return new StrictMobilityOverride(
            code,
            title,
            "",
            0,
            "",
            "",
            0,
            0.0,
            0.0,
            0,
            StrictMobilityOverrideStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictMobilityOverrideStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictMobilityOverrideStatus next)
    {
        switch (Status)
        {
            case StrictMobilityOverrideStatus.Requested:
                switch (next)
                {
                    case StrictMobilityOverrideStatus.Granted:
                    case StrictMobilityOverrideStatus.Expired:
                        return true;
                }
                return false;
            case StrictMobilityOverrideStatus.Granted:
                switch (next)
                {
                    case StrictMobilityOverrideStatus.Revoked:
                        return true;
                }
                return false;
            case StrictMobilityOverrideStatus.Revoked:
                switch (next)
                {
                    case StrictMobilityOverrideStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictMobilityOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict mobility override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Revision + Ceiling, 2);
    }

    public StrictMobilityOverride Copy()
    {
        return new StrictMobilityOverride(
            Code,
            Title,
            Currency,
            Priority,
            Label,
            Segment,
            DurationMinutes,
            Ceiling,
            Maximum,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
