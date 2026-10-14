namespace Acme.Workforce.Talent.Pool.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool mapping is in its life cycle.</summary>
public enum PoolMappingStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One pool mapping, with what the planners need to act on it.</summary>
public sealed class PoolMapping(string code, string name, double cost, int revision, int graceDays, string jobCode, double threshold, double discount, string country, bool visible, string label, PoolMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public int Revision { get; set; } = revision;
    public int GraceDays { get; set; } = graceDays;
    public string JobCode { get; set; } = jobCode;
    public double Threshold { get; set; } = threshold;
    public double Discount { get; set; } = discount;
    public string Country { get; set; } = country;
    public bool Visible { get; set; } = visible;
    public string Label { get; set; } = label;
    public PoolMappingStatus Status { get; set; } = status;

    /// <summary>Starts a pool mapping in status proposed.</summary>
    public static PoolMapping NewPoolMapping(string code, string name)
    {
        return new PoolMapping(code, name, 0.0, 0, 0, "", 0.0, 0.0, "", false, "", PoolMappingStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolMappingStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolMappingStatus next)
    {
        switch (Status)
        {
            case PoolMappingStatus.Proposed:
                return next == PoolMappingStatus.Approved || next == PoolMappingStatus.Rejected;
            case PoolMappingStatus.Approved:
                switch (next)
                {
                    case PoolMappingStatus.Withdrawn:
                        return true;
                }
                return false;
            case PoolMappingStatus.Rejected:
                switch (next)
                {
                    case PoolMappingStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"pool mapping"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Revision + Discount, 2);
    }

    public PoolMapping Copy()
    {
        return new PoolMapping(
            Code,
            Name,
            Cost,
            Revision,
            GraceDays,
            JobCode,
            Threshold,
            Discount,
            Country,
            Visible,
            Label,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
