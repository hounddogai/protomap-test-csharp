namespace Acme.Workforce.Compensation.PayBand.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default pay band allocation is in its life cycle.</summary>
public enum DefaultPayBandAllocationStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A default pay band allocation as the pay band team keeps it.</summary>
public sealed class DefaultPayBandAllocation(string code, string name, string category, string region, double maximum, string department, int version, DefaultPayBandAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public string Region { get; set; } = region;
    public double Maximum { get; set; } = maximum;
    public string Department { get; set; } = department;
    public int Version { get; set; } = version;
    public DefaultPayBandAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a default pay band allocation in status draft.</summary>
    public static DefaultPayBandAllocation NewDefaultPayBandAllocation(string code, string name)
    {
        return new DefaultPayBandAllocation(code, name, "", "", 0.0, "", 0, DefaultPayBandAllocationStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPayBandAllocationStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPayBandAllocationStatus next)
    {
        switch (Status)
        {
            case DefaultPayBandAllocationStatus.Draft:
                switch (next)
                {
                    case DefaultPayBandAllocationStatus.Suspended:
                        return true;
                }
                return false;
            case DefaultPayBandAllocationStatus.Pending:
                return next == DefaultPayBandAllocationStatus.Closed;
            case DefaultPayBandAllocationStatus.Active:
                switch (next)
                {
                    case DefaultPayBandAllocationStatus.Suspended:
                    case DefaultPayBandAllocationStatus.Closed:
                        return true;
                }
                return false;
            case DefaultPayBandAllocationStatus.Suspended:
                switch (next)
                {
                    case DefaultPayBandAllocationStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultPayBandAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default pay band allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Version + Maximum, 2);
    }

    public DefaultPayBandAllocation Copy()
    {
        return new DefaultPayBandAllocation(Code, Name, Category, Region, Maximum, Department, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
