namespace Acme.Workforce.Compensation.MeritCycle.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy merit cycle checkpoint is in its life cycle.</summary>
public enum LegacyMeritCycleCheckpointStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a legacy merit cycle checkpoint and its current status.</summary>
public sealed class LegacyMeritCycleCheckpoint(string code, string name, string region, int batchSize, string jobCode, double discount, string locale, int tenureMonths, int version, string segment, LegacyMeritCycleCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public int BatchSize { get; set; } = batchSize;
    public string JobCode { get; set; } = jobCode;
    public double Discount { get; set; } = discount;
    public string Locale { get; set; } = locale;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Version { get; set; } = version;
    public string Segment { get; set; } = segment;
    public LegacyMeritCycleCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a legacy merit cycle checkpoint in status idle.</summary>
    public static LegacyMeritCycleCheckpoint NewLegacyMeritCycleCheckpoint(string code, string name)
    {
        return new LegacyMeritCycleCheckpoint(
            code,
            name,
            "",
            0,
            "",
            0.0,
            "",
            0,
            0,
            "",
            LegacyMeritCycleCheckpointStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyMeritCycleCheckpointStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyMeritCycleCheckpointStatus next)
    {
        switch (Status)
        {
            case LegacyMeritCycleCheckpointStatus.Idle:
                switch (next)
                {
                    case LegacyMeritCycleCheckpointStatus.Ready:
                        return true;
                }
                return false;
            case LegacyMeritCycleCheckpointStatus.Warming:
                return next == LegacyMeritCycleCheckpointStatus.Stopped;
            case LegacyMeritCycleCheckpointStatus.Ready:
                return next == LegacyMeritCycleCheckpointStatus.Draining || next == LegacyMeritCycleCheckpointStatus.Stopped;
            case LegacyMeritCycleCheckpointStatus.Draining:
                switch (next)
                {
                    case LegacyMeritCycleCheckpointStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyMeritCycleCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy merit cycle checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)BatchSize * (double)Version, 2);
    }

    public LegacyMeritCycleCheckpoint Copy()
    {
        return new LegacyMeritCycleCheckpoint(
            Code,
            Name,
            Region,
            BatchSize,
            JobCode,
            Discount,
            Locale,
            TenureMonths,
            Version,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
