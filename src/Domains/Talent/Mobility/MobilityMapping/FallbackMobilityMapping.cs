namespace Acme.Workforce.Talent.Mobility.MobilityMapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback mobility mapping is in its life cycle.</summary>
public enum FallbackMobilityMappingStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a fallback mobility mapping and its current status.</summary>
public sealed class FallbackMobilityMapping(string code, string label, string contactEmail, string segment, int limit, string checksum, int tenureMonths, string department, double amount, string name, FallbackMobilityMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ContactEmail { get; set; } = contactEmail;
    public string Segment { get; set; } = segment;
    public int Limit { get; set; } = limit;
    public string Checksum { get; set; } = checksum;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Department { get; set; } = department;
    public double Amount { get; set; } = amount;
    public string Name { get; set; } = name;
    public FallbackMobilityMappingStatus Status { get; set; } = status;

    /// <summary>Starts a fallback mobility mapping in status planned.</summary>
    public static FallbackMobilityMapping NewFallbackMobilityMapping(string code, string label)
    {
        return new FallbackMobilityMapping(
            code,
            label,
            "",
            "",
            0,
            "",
            0,
            "",
            0.0,
            "",
            FallbackMobilityMappingStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackMobilityMappingStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackMobilityMappingStatus next)
    {
        switch (Status)
        {
            case FallbackMobilityMappingStatus.Planned:
                switch (next)
                {
                    case FallbackMobilityMappingStatus.Cancelled:
                        return true;
                }
                return false;
            case FallbackMobilityMappingStatus.Scheduled:
                return next == FallbackMobilityMappingStatus.InProgress || next == FallbackMobilityMappingStatus.Completed;
            case FallbackMobilityMappingStatus.InProgress:
                switch (next)
                {
                    case FallbackMobilityMappingStatus.Completed:
                    case FallbackMobilityMappingStatus.Cancelled:
                        return true;
                }
                return false;
            case FallbackMobilityMappingStatus.Completed:
                return next == FallbackMobilityMappingStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(FallbackMobilityMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback mobility mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"fallback mobility mapping"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Amount - (double)TenureMonths, 2);
    }

    public FallbackMobilityMapping Copy()
    {
        return new FallbackMobilityMapping(
            Code,
            Label,
            ContactEmail,
            Segment,
            Limit,
            Checksum,
            TenureMonths,
            Department,
            Amount,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
