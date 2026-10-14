namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending occupancy notice is in its life cycle.</summary>
public enum PendingOccupancyNoticeStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pending occupancy notice as the occupancy team keeps it.</summary>
public sealed class PendingOccupancyNotice(string code, string name, int tenureMonths, string salaryBand, int attempts, string externalRef, string ownerId, double floorValue, string notes, string summary, PendingOccupancyNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int TenureMonths { get; set; } = tenureMonths;
    public string SalaryBand { get; set; } = salaryBand;
    public int Attempts { get; set; } = attempts;
    public string ExternalRef { get; set; } = externalRef;
    public string OwnerId { get; set; } = ownerId;
    public double FloorValue { get; set; } = floorValue;
    public string Notes { get; set; } = notes;
    public string Summary { get; set; } = summary;
    public PendingOccupancyNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a pending occupancy notice in status requested.</summary>
    public static PendingOccupancyNotice NewPendingOccupancyNotice(string code, string name)
    {
        return new PendingOccupancyNotice(
            code,
            name,
            0,
            "",
            0,
            "",
            "",
            0.0,
            "",
            "",
            PendingOccupancyNoticeStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingOccupancyNoticeStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingOccupancyNoticeStatus next)
    {
        switch (Status)
        {
            case PendingOccupancyNoticeStatus.Requested:
                switch (next)
                {
                    case PendingOccupancyNoticeStatus.Revoked:
                    case PendingOccupancyNoticeStatus.Expired:
                        return true;
                }
                return false;
            case PendingOccupancyNoticeStatus.Granted:
                return next == PendingOccupancyNoticeStatus.Revoked;
            case PendingOccupancyNoticeStatus.Revoked:
                return next == PendingOccupancyNoticeStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PendingOccupancyNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending occupancy notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)TenureMonths + (double)Attempts, 2);
    }

    public PendingOccupancyNotice Copy()
    {
        return new PendingOccupancyNotice(
            Code,
            Name,
            TenureMonths,
            SalaryBand,
            Attempts,
            ExternalRef,
            OwnerId,
            FloorValue,
            Notes,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
