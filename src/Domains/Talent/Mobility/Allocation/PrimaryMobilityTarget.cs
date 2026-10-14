namespace Acme.Workforce.Talent.Mobility.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary mobility target is in its life cycle.</summary>
public enum PrimaryMobilityTargetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A primary mobility target as the mobility team keeps it.</summary>
public sealed class PrimaryMobilityTarget(string code, string title, string description, string channel, string country, double minimum, string jobCode, int capacity, PrimaryMobilityTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public string Channel { get; set; } = channel;
    public string Country { get; set; } = country;
    public double Minimum { get; set; } = minimum;
    public string JobCode { get; set; } = jobCode;
    public int Capacity { get; set; } = capacity;
    public PrimaryMobilityTargetStatus Status { get; set; } = status;

    /// <summary>Starts a primary mobility target in status draft.</summary>
    public static PrimaryMobilityTarget NewPrimaryMobilityTarget(string code, string title)
    {
        return new PrimaryMobilityTarget(code, title, "", "", "", 0.0, "", 0, PrimaryMobilityTargetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryMobilityTargetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryMobilityTargetStatus next)
    {
        switch (Status)
        {
            case PrimaryMobilityTargetStatus.Draft:
                return next == PrimaryMobilityTargetStatus.Suspended;
            case PrimaryMobilityTargetStatus.Pending:
                return next == PrimaryMobilityTargetStatus.Active || next == PrimaryMobilityTargetStatus.Suspended;
            case PrimaryMobilityTargetStatus.Active:
                switch (next)
                {
                    case PrimaryMobilityTargetStatus.Suspended:
                    case PrimaryMobilityTargetStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryMobilityTargetStatus.Suspended:
                return next == PrimaryMobilityTargetStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PrimaryMobilityTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary mobility target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedExposure()
    {
        return Math.Round(Minimum - (double)Capacity, 2);
    }

    public PrimaryMobilityTarget Copy()
    {
        return new PrimaryMobilityTarget(
            Code,
            Title,
            Description,
            Channel,
            Country,
            Minimum,
            JobCode,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
