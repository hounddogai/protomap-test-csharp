namespace Acme.Workforce.Workplace.Occupancy.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional occupancy tier is in its life cycle.</summary>
public enum RegionalOccupancyTierStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A regional occupancy tier as the occupancy team keeps it.</summary>
public sealed class RegionalOccupancyTier(string code, string name, int leaveDays, string location, string costCenter, bool pinned, string badge, double rate, string managerId, string team, RegionalOccupancyTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public string Location { get; set; } = location;
    public string CostCenter { get; set; } = costCenter;
    public bool Pinned { get; set; } = pinned;
    public string Badge { get; set; } = badge;
    public double Rate { get; set; } = rate;
    public string ManagerId { get; set; } = managerId;
    public string Team { get; set; } = team;
    public RegionalOccupancyTierStatus Status { get; set; } = status;

    /// <summary>Starts a regional occupancy tier in status created.</summary>
    public static RegionalOccupancyTier NewRegionalOccupancyTier(string code, string name)
    {
        return new RegionalOccupancyTier(
            code,
            name,
            0,
            "",
            "",
            false,
            "",
            0.0,
            "",
            "",
            RegionalOccupancyTierStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalOccupancyTierStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalOccupancyTierStatus next)
    {
        switch (Status)
        {
            case RegionalOccupancyTierStatus.Created:
                return next == RegionalOccupancyTierStatus.Validated || next == RegionalOccupancyTierStatus.Published;
            case RegionalOccupancyTierStatus.Validated:
                return next == RegionalOccupancyTierStatus.Published;
            case RegionalOccupancyTierStatus.Published:
                return next == RegionalOccupancyTierStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RegionalOccupancyTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional occupancy tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetLoad()
    {
        return Math.Round((double)LeaveDays * Rate, 2);
    }

    public RegionalOccupancyTier Copy()
    {
        return new RegionalOccupancyTier(
            Code,
            Name,
            LeaveDays,
            Location,
            CostCenter,
            Pinned,
            Badge,
            Rate,
            ManagerId,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
