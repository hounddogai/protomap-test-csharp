namespace Acme.Workforce.Orgdesign.CostModel.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback cost model rollup is in its life cycle.</summary>
public enum FallbackCostModelRollupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A fallback cost model rollup as the cost model team keeps it.</summary>
public sealed class FallbackCostModelRollup(string code, string name, double hours, int attempts, int priority, string source, FallbackCostModelRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Hours { get; set; } = hours;
    public int Attempts { get; set; } = attempts;
    public int Priority { get; set; } = priority;
    public string Source { get; set; } = source;
    public FallbackCostModelRollupStatus Status { get; set; } = status;

    /// <summary>Starts a fallback cost model rollup in status created.</summary>
    public static FallbackCostModelRollup NewFallbackCostModelRollup(string code, string name)
    {
        return new FallbackCostModelRollup(code, name, 0.0, 0, 0, "", FallbackCostModelRollupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackCostModelRollupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackCostModelRollupStatus next)
    {
        switch (Status)
        {
            case FallbackCostModelRollupStatus.Created:
                switch (next)
                {
                    case FallbackCostModelRollupStatus.Published:
                    case FallbackCostModelRollupStatus.Retired:
                        return true;
                }
                return false;
            case FallbackCostModelRollupStatus.Validated:
                switch (next)
                {
                    case FallbackCostModelRollupStatus.Published:
                    case FallbackCostModelRollupStatus.Retired:
                        return true;
                }
                return false;
            case FallbackCostModelRollupStatus.Published:
                return next == FallbackCostModelRollupStatus.Retired;
        }
        return false;
    }

    public void MoveTo(FallbackCostModelRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback cost model rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Hours - (double)Priority, 2);
    }

    public FallbackCostModelRollup Copy()
    {
        return new FallbackCostModelRollup(Code, Name, Hours, Attempts, Priority, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
