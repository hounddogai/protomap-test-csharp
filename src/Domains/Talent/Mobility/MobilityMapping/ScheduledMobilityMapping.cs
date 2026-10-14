namespace Acme.Workforce.Talent.Mobility.MobilityMapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled mobility mapping is in its life cycle.</summary>
public enum ScheduledMobilityMappingStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a scheduled mobility mapping and its current status.</summary>
public sealed class ScheduledMobilityMapping(string code, string name, double weight, int tenureMonths, double hours, int quantity, double minimum, double discount, string checksum, int limit, ScheduledMobilityMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Hours { get; set; } = hours;
    public int Quantity { get; set; } = quantity;
    public double Minimum { get; set; } = minimum;
    public double Discount { get; set; } = discount;
    public string Checksum { get; set; } = checksum;
    public int Limit { get; set; } = limit;
    public ScheduledMobilityMappingStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled mobility mapping in status idle.</summary>
    public static ScheduledMobilityMapping NewScheduledMobilityMapping(string code, string name)
    {
        return new ScheduledMobilityMapping(
            code,
            name,
            0.0,
            0,
            0.0,
            0,
            0.0,
            0.0,
            "",
            0,
            ScheduledMobilityMappingStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledMobilityMappingStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledMobilityMappingStatus next)
    {
        switch (Status)
        {
            case ScheduledMobilityMappingStatus.Idle:
                switch (next)
                {
                    case ScheduledMobilityMappingStatus.Draining:
                    case ScheduledMobilityMappingStatus.Stopped:
                        return true;
                }
                return false;
            case ScheduledMobilityMappingStatus.Warming:
                return next == ScheduledMobilityMappingStatus.Stopped;
            case ScheduledMobilityMappingStatus.Ready:
                return next == ScheduledMobilityMappingStatus.Draining;
            case ScheduledMobilityMappingStatus.Draining:
                return next == ScheduledMobilityMappingStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ScheduledMobilityMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled mobility mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled mobility mapping"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Hours - Minimum, 2);
    }

    public ScheduledMobilityMapping Copy()
    {
        return new ScheduledMobilityMapping(
            Code,
            Name,
            Weight,
            TenureMonths,
            Hours,
            Quantity,
            Minimum,
            Discount,
            Checksum,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
