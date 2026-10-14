namespace Acme.Workforce.Workplace.Occupancy.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy mapping is in its life cycle.</summary>
public enum OccupancyMappingStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A occupancy mapping. Its code is unique within a tenant.</summary>
public sealed class OccupancyMapping(string code, string name, int version, double score, bool visible, string locale, string timezone, string employeeId, string currency, double maximum, double cost, OccupancyMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public double Score { get; set; } = score;
    public bool Visible { get; set; } = visible;
    public string Locale { get; set; } = locale;
    public string Timezone { get; set; } = timezone;
    public string EmployeeId { get; set; } = employeeId;
    public string Currency { get; set; } = currency;
    public double Maximum { get; set; } = maximum;
    public double Cost { get; set; } = cost;
    public OccupancyMappingStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy mapping in status idle.</summary>
    public static OccupancyMapping NewOccupancyMapping(string code, string name)
    {
        return new OccupancyMapping(
            code,
            name,
            0,
            0.0,
            false,
            "",
            "",
            "",
            "",
            0.0,
            0.0,
            OccupancyMappingStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyMappingStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyMappingStatus next)
    {
        switch (Status)
        {
            case OccupancyMappingStatus.Idle:
                return next == OccupancyMappingStatus.Stopped;
            case OccupancyMappingStatus.Warming:
                switch (next)
                {
                    case OccupancyMappingStatus.Ready:
                    case OccupancyMappingStatus.Draining:
                        return true;
                }
                return false;
            case OccupancyMappingStatus.Ready:
                switch (next)
                {
                    case OccupancyMappingStatus.Draining:
                    case OccupancyMappingStatus.Stopped:
                        return true;
                }
                return false;
            case OccupancyMappingStatus.Draining:
                switch (next)
                {
                    case OccupancyMappingStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"occupancy mapping"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Cost - (double)Version, 2);
    }

    public OccupancyMapping Copy()
    {
        return new OccupancyMapping(
            Code,
            Name,
            Version,
            Score,
            Visible,
            Locale,
            Timezone,
            EmployeeId,
            Currency,
            Maximum,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
