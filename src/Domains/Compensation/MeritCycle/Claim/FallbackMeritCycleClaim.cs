namespace Acme.Workforce.Compensation.MeritCycle.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback merit cycle claim is in its life cycle.</summary>
public enum FallbackMeritCycleClaimStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One fallback merit cycle claim, with what the planners need to act on it.</summary>
public sealed class FallbackMeritCycleClaim(string code, string label, string segment, List<string> tags, string source, int priority, string timezone, string currency, FallbackMeritCycleClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public List<string> Tags { get; set; } = tags;
    public string Source { get; set; } = source;
    public int Priority { get; set; } = priority;
    public string Timezone { get; set; } = timezone;
    public string Currency { get; set; } = currency;
    public FallbackMeritCycleClaimStatus Status { get; set; } = status;

    /// <summary>Starts a fallback merit cycle claim in status draft.</summary>
    public static FallbackMeritCycleClaim NewFallbackMeritCycleClaim(string code, string label)
    {
        return new FallbackMeritCycleClaim(
            code,
            label,
            "",
            new List<string>(),
            "",
            0,
            "",
            "",
            FallbackMeritCycleClaimStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackMeritCycleClaimStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackMeritCycleClaimStatus next)
    {
        switch (Status)
        {
            case FallbackMeritCycleClaimStatus.Draft:
                switch (next)
                {
                    case FallbackMeritCycleClaimStatus.Pending:
                    case FallbackMeritCycleClaimStatus.Closed:
                        return true;
                }
                return false;
            case FallbackMeritCycleClaimStatus.Pending:
                switch (next)
                {
                    case FallbackMeritCycleClaimStatus.Closed:
                        return true;
                }
                return false;
            case FallbackMeritCycleClaimStatus.Active:
                switch (next)
                {
                    case FallbackMeritCycleClaimStatus.Suspended:
                    case FallbackMeritCycleClaimStatus.Closed:
                        return true;
                }
                return false;
            case FallbackMeritCycleClaimStatus.Suspended:
                return next == FallbackMeritCycleClaimStatus.Closed;
        }
        return false;
    }

    public void MoveTo(FallbackMeritCycleClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback merit cycle claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return (double)Priority;
    }

    public FallbackMeritCycleClaim Copy()
    {
        return new FallbackMeritCycleClaim(Code, Label, Segment, Tags, Source, Priority, Timezone, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
