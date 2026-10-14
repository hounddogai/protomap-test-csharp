namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy manifest is in its life cycle.</summary>
public enum OccupancyManifestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A occupancy manifest as the occupancy team keeps it.</summary>
public sealed class OccupancyManifest(string code, string name, int capacity, int durationMinutes, string salaryBand, string category, double amount, string region, OccupancyManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string SalaryBand { get; set; } = salaryBand;
    public string Category { get; set; } = category;
    public double Amount { get; set; } = amount;
    public string Region { get; set; } = region;
    public OccupancyManifestStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy manifest in status draft.</summary>
    public static OccupancyManifest NewOccupancyManifest(string code, string name)
    {
        return new OccupancyManifest(code, name, 0, 0, "", "", 0.0, "", OccupancyManifestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyManifestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyManifestStatus next)
    {
        switch (Status)
        {
            case OccupancyManifestStatus.Draft:
                return next == OccupancyManifestStatus.Active || next == OccupancyManifestStatus.Suspended;
            case OccupancyManifestStatus.Pending:
                switch (next)
                {
                    case OccupancyManifestStatus.Active:
                        return true;
                }
                return false;
            case OccupancyManifestStatus.Active:
                return next == OccupancyManifestStatus.Suspended || next == OccupancyManifestStatus.Closed;
            case OccupancyManifestStatus.Suspended:
                switch (next)
                {
                    case OccupancyManifestStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Capacity - Amount, 2);
    }

    public OccupancyManifest Copy()
    {
        return new OccupancyManifest(
            Code,
            Name,
            Capacity,
            DurationMinutes,
            SalaryBand,
            Category,
            Amount,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
