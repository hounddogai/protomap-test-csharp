namespace Acme.Workforce.Payrollops.Garnishment.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment mapping is in its life cycle.</summary>
public enum GarnishmentMappingStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a garnishment mapping and its current status.</summary>
public sealed class GarnishmentMapping(string code, string name, double maximum, bool visible, double discount, bool approved, string country, int version, GarnishmentMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public bool Visible { get; set; } = visible;
    public double Discount { get; set; } = discount;
    public bool Approved { get; set; } = approved;
    public string Country { get; set; } = country;
    public int Version { get; set; } = version;
    public GarnishmentMappingStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment mapping in status planned.</summary>
    public static GarnishmentMapping NewGarnishmentMapping(string code, string name)
    {
        return new GarnishmentMapping(code, name, 0.0, false, 0.0, false, "", 0, GarnishmentMappingStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentMappingStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentMappingStatus next)
    {
        switch (Status)
        {
            case GarnishmentMappingStatus.Planned:
                switch (next)
                {
                    case GarnishmentMappingStatus.Cancelled:
                        return true;
                }
                return false;
            case GarnishmentMappingStatus.Scheduled:
                switch (next)
                {
                    case GarnishmentMappingStatus.InProgress:
                    case GarnishmentMappingStatus.Cancelled:
                        return true;
                }
                return false;
            case GarnishmentMappingStatus.InProgress:
                switch (next)
                {
                    case GarnishmentMappingStatus.Completed:
                    case GarnishmentMappingStatus.Cancelled:
                        return true;
                }
                return false;
            case GarnishmentMappingStatus.Completed:
                switch (next)
                {
                    case GarnishmentMappingStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"garnishment mapping"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Version - Discount, 2);
    }

    public GarnishmentMapping Copy()
    {
        return new GarnishmentMapping(Code, Name, Maximum, Visible, Discount, Approved, Country, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
