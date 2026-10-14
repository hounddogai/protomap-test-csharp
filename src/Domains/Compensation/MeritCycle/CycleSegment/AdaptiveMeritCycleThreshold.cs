namespace Acme.Workforce.Compensation.MeritCycle.CycleSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive merit cycle threshold is in its life cycle.</summary>
public enum AdaptiveMeritCycleThresholdStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A adaptive merit cycle threshold. Its code is unique within a tenant.</summary>
public sealed class AdaptiveMeritCycleThreshold(string code, string name, bool visible, double fte, List<string> tags, string currency, int limit, AdaptiveMeritCycleThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public double Fte { get; set; } = fte;
    public List<string> Tags { get; set; } = tags;
    public string Currency { get; set; } = currency;
    public int Limit { get; set; } = limit;
    public AdaptiveMeritCycleThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive merit cycle threshold in status requested.</summary>
    public static AdaptiveMeritCycleThreshold NewAdaptiveMeritCycleThreshold(string code, string name)
    {
        return new AdaptiveMeritCycleThreshold(
            code,
            name,
            false,
            0.0,
            new List<string>(),
            "",
            0,
            AdaptiveMeritCycleThresholdStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveMeritCycleThresholdStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveMeritCycleThresholdStatus next)
    {
        switch (Status)
        {
            case AdaptiveMeritCycleThresholdStatus.Requested:
                switch (next)
                {
                    case AdaptiveMeritCycleThresholdStatus.Revoked:
                    case AdaptiveMeritCycleThresholdStatus.Expired:
                        return true;
                }
                return false;
            case AdaptiveMeritCycleThresholdStatus.Granted:
                return next == AdaptiveMeritCycleThresholdStatus.Revoked || next == AdaptiveMeritCycleThresholdStatus.Expired;
            case AdaptiveMeritCycleThresholdStatus.Revoked:
                switch (next)
                {
                    case AdaptiveMeritCycleThresholdStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveMeritCycleThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive merit cycle threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"adaptive merit cycle threshold"} {Code}";
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

    public double NetLoad()
    {
        return Math.Round(Fte - (double)Limit, 2);
    }

    public AdaptiveMeritCycleThreshold Copy()
    {
        return new AdaptiveMeritCycleThreshold(Code, Name, Visible, Fte, Tags, Currency, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
