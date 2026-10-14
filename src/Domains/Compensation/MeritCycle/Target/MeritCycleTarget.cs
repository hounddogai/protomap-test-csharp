namespace Acme.Workforce.Compensation.MeritCycle.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle target is in its life cycle.</summary>
public enum MeritCycleTargetStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A merit cycle target. Its code is unique within a tenant.</summary>
public sealed class MeritCycleTarget(string code, string name, string timezone, string contactEmail, string segment, double rating, int batchSize, double minimum, double floorValue, string title, string region, MeritCycleTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Timezone { get; set; } = timezone;
    public string ContactEmail { get; set; } = contactEmail;
    public string Segment { get; set; } = segment;
    public double Rating { get; set; } = rating;
    public int BatchSize { get; set; } = batchSize;
    public double Minimum { get; set; } = minimum;
    public double FloorValue { get; set; } = floorValue;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public MeritCycleTargetStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle target in status requested.</summary>
    public static MeritCycleTarget NewMeritCycleTarget(string code, string name)
    {
        return new MeritCycleTarget(
            code,
            name,
            "",
            "",
            "",
            0.0,
            0,
            0.0,
            0.0,
            "",
            "",
            MeritCycleTargetStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleTargetStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleTargetStatus next)
    {
        switch (Status)
        {
            case MeritCycleTargetStatus.Requested:
                return next == MeritCycleTargetStatus.Revoked;
            case MeritCycleTargetStatus.Granted:
                switch (next)
                {
                    case MeritCycleTargetStatus.Expired:
                        return true;
                }
                return false;
            case MeritCycleTargetStatus.Revoked:
                return next == MeritCycleTargetStatus.Expired;
        }
        return false;
    }

    public void MoveTo(MeritCycleTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"merit cycle target"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Minimum + Rating, 2);
    }

    public MeritCycleTarget Copy()
    {
        return new MeritCycleTarget(
            Code,
            Name,
            Timezone,
            ContactEmail,
            Segment,
            Rating,
            BatchSize,
            Minimum,
            FloorValue,
            Title,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
