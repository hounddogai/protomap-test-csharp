namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy record is in its life cycle.</summary>
public enum OccupancyRecordStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a occupancy record and its current status.</summary>
public sealed class OccupancyRecord(string code, string title, double rate, string department, double budget, double margin, string team, string managerId, int tenureMonths, double weight, double rating, OccupancyRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public string Department { get; set; } = department;
    public double Budget { get; set; } = budget;
    public double Margin { get; set; } = margin;
    public string Team { get; set; } = team;
    public string ManagerId { get; set; } = managerId;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Weight { get; set; } = weight;
    public double Rating { get; set; } = rating;
    public OccupancyRecordStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy record in status planned.</summary>
    public static OccupancyRecord NewOccupancyRecord(string code, string title)
    {
        return new OccupancyRecord(
            code,
            title,
            0.0,
            "",
            0.0,
            0.0,
            "",
            "",
            0,
            0.0,
            0.0,
            OccupancyRecordStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyRecordStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyRecordStatus next)
    {
        switch (Status)
        {
            case OccupancyRecordStatus.Planned:
                switch (next)
                {
                    case OccupancyRecordStatus.InProgress:
                        return true;
                }
                return false;
            case OccupancyRecordStatus.Scheduled:
                switch (next)
                {
                    case OccupancyRecordStatus.Cancelled:
                        return true;
                }
                return false;
            case OccupancyRecordStatus.InProgress:
                return next == OccupancyRecordStatus.Cancelled;
            case OccupancyRecordStatus.Completed:
                return next == OccupancyRecordStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(OccupancyRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Margin - Budget, 2);
    }

    public OccupancyRecord Copy()
    {
        return new OccupancyRecord(
            Code,
            Title,
            Rate,
            Department,
            Budget,
            Margin,
            Team,
            ManagerId,
            TenureMonths,
            Weight,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
