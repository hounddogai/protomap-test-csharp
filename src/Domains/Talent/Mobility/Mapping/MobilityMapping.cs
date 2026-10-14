namespace Acme.Workforce.Talent.Mobility.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility mapping is in its life cycle.</summary>
public enum MobilityMappingStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One mobility mapping, with what the planners need to act on it.</summary>
public sealed class MobilityMapping(string code, string title, int limit, bool active, string location, string employeeId, int revision, double rating, double amount, MobilityMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public bool Active { get; set; } = active;
    public string Location { get; set; } = location;
    public string EmployeeId { get; set; } = employeeId;
    public int Revision { get; set; } = revision;
    public double Rating { get; set; } = rating;
    public double Amount { get; set; } = amount;
    public MobilityMappingStatus Status { get; set; } = status;

    /// <summary>Starts a mobility mapping in status planned.</summary>
    public static MobilityMapping NewMobilityMapping(string code, string title)
    {
        return new MobilityMapping(code, title, 0, false, "", "", 0, 0.0, 0.0, MobilityMappingStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityMappingStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityMappingStatus next)
    {
        switch (Status)
        {
            case MobilityMappingStatus.Planned:
                return next == MobilityMappingStatus.Scheduled || next == MobilityMappingStatus.InProgress;
            case MobilityMappingStatus.Scheduled:
                switch (next)
                {
                    case MobilityMappingStatus.Completed:
                    case MobilityMappingStatus.Cancelled:
                        return true;
                }
                return false;
            case MobilityMappingStatus.InProgress:
                switch (next)
                {
                    case MobilityMappingStatus.Completed:
                        return true;
                }
                return false;
            case MobilityMappingStatus.Completed:
                switch (next)
                {
                    case MobilityMappingStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MobilityMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"mobility mapping"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Limit + Amount, 2);
    }

    public MobilityMapping Copy()
    {
        return new MobilityMapping(
            Code,
            Title,
            Limit,
            Active,
            Location,
            EmployeeId,
            Revision,
            Rating,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
