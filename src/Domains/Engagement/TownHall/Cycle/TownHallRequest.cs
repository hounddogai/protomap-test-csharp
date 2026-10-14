namespace Acme.Workforce.Engagement.TownHall.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall request is in its life cycle.</summary>
public enum TownHallRequestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A town hall request as the town hall team keeps it.</summary>
public sealed class TownHallRequest(string code, string label, int createdAt, string phone, int expiresAt, string notes, string grade, string location, int capacity, double floorValue, bool archived, TownHallRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public string Phone { get; set; } = phone;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Notes { get; set; } = notes;
    public string Grade { get; set; } = grade;
    public string Location { get; set; } = location;
    public int Capacity { get; set; } = capacity;
    public double FloorValue { get; set; } = floorValue;
    public bool Archived { get; set; } = archived;
    public TownHallRequestStatus Status { get; set; } = status;

    /// <summary>Starts a town hall request in status planned.</summary>
    public static TownHallRequest NewTownHallRequest(string code, string label)
    {
        return new TownHallRequest(code, label, 0, "", 0, "", "", "", 0, 0.0, false, TownHallRequestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallRequestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallRequestStatus next)
    {
        switch (Status)
        {
            case TownHallRequestStatus.Planned:
                return next == TownHallRequestStatus.Completed;
            case TownHallRequestStatus.Scheduled:
                switch (next)
                {
                    case TownHallRequestStatus.InProgress:
                    case TownHallRequestStatus.Completed:
                        return true;
                }
                return false;
            case TownHallRequestStatus.InProgress:
                return next == TownHallRequestStatus.Cancelled;
            case TownHallRequestStatus.Completed:
                switch (next)
                {
                    case TownHallRequestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Capacity + (double)CreatedAt, 2);
    }

    public TownHallRequest Copy()
    {
        return new TownHallRequest(
            Code,
            Label,
            CreatedAt,
            Phone,
            ExpiresAt,
            Notes,
            Grade,
            Location,
            Capacity,
            FloorValue,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
