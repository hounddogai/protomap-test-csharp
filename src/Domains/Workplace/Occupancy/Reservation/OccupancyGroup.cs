namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy group is in its life cycle.</summary>
public enum OccupancyGroupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One occupancy group, with what the planners need to act on it.</summary>
public sealed class OccupancyGroup(string code, string name, double margin, int tenureMonths, string managerId, int startsAt, double ratio, string costCenter, string jobCode, string location, OccupancyGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public int TenureMonths { get; set; } = tenureMonths;
    public string ManagerId { get; set; } = managerId;
    public int StartsAt { get; set; } = startsAt;
    public double Ratio { get; set; } = ratio;
    public string CostCenter { get; set; } = costCenter;
    public string JobCode { get; set; } = jobCode;
    public string Location { get; set; } = location;
    public OccupancyGroupStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy group in status created.</summary>
    public static OccupancyGroup NewOccupancyGroup(string code, string name)
    {
        return new OccupancyGroup(code, name, 0.0, 0, "", 0, 0.0, "", "", "", OccupancyGroupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyGroupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyGroupStatus next)
    {
        switch (Status)
        {
            case OccupancyGroupStatus.Created:
                switch (next)
                {
                    case OccupancyGroupStatus.Validated:
                    case OccupancyGroupStatus.Published:
                        return true;
                }
                return false;
            case OccupancyGroupStatus.Validated:
                return next == OccupancyGroupStatus.Retired;
            case OccupancyGroupStatus.Published:
                return next == OccupancyGroupStatus.Retired;
        }
        return false;
    }

    public void MoveTo(OccupancyGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedExposure()
    {
        return Math.Round(Margin * (double)StartsAt, 2);
    }

    public OccupancyGroup Copy()
    {
        return new OccupancyGroup(
            Code,
            Name,
            Margin,
            TenureMonths,
            ManagerId,
            StartsAt,
            Ratio,
            CostCenter,
            JobCode,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
