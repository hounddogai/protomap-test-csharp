namespace Acme.Workforce.Workplace.Occupancy.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy ledger is in its life cycle.</summary>
public enum OccupancyLedgerStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a occupancy ledger and its current status.</summary>
public sealed class OccupancyLedger(string code, string title, string category, double rating, int leaveDays, string channel, OccupancyLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public double Rating { get; set; } = rating;
    public int LeaveDays { get; set; } = leaveDays;
    public string Channel { get; set; } = channel;
    public OccupancyLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy ledger in status planned.</summary>
    public static OccupancyLedger NewOccupancyLedger(string code, string title)
    {
        return new OccupancyLedger(code, title, "", 0.0, 0, "", OccupancyLedgerStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyLedgerStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyLedgerStatus next)
    {
        switch (Status)
        {
            case OccupancyLedgerStatus.Planned:
                return next == OccupancyLedgerStatus.Scheduled;
            case OccupancyLedgerStatus.Scheduled:
                switch (next)
                {
                    case OccupancyLedgerStatus.Completed:
                    case OccupancyLedgerStatus.Cancelled:
                        return true;
                }
                return false;
            case OccupancyLedgerStatus.InProgress:
                switch (next)
                {
                    case OccupancyLedgerStatus.Cancelled:
                        return true;
                }
                return false;
            case OccupancyLedgerStatus.Completed:
                return next == OccupancyLedgerStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(OccupancyLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Rating * (double)LeaveDays, 2);
    }

    public OccupancyLedger Copy()
    {
        return new OccupancyLedger(Code, Title, Category, Rating, LeaveDays, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
