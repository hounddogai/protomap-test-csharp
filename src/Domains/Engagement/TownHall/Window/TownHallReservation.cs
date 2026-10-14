namespace Acme.Workforce.Engagement.TownHall.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall reservation is in its life cycle.</summary>
public enum TownHallReservationStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a town hall reservation and its current status.</summary>
public sealed class TownHallReservation(string code, string label, string location, string contactEmail, int retries, double budget, string currency, int createdAt, int batchSize, TownHallReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public string ContactEmail { get; set; } = contactEmail;
    public int Retries { get; set; } = retries;
    public double Budget { get; set; } = budget;
    public string Currency { get; set; } = currency;
    public int CreatedAt { get; set; } = createdAt;
    public int BatchSize { get; set; } = batchSize;
    public TownHallReservationStatus Status { get; set; } = status;

    /// <summary>Starts a town hall reservation in status new.</summary>
    public static TownHallReservation NewTownHallReservation(string code, string label)
    {
        return new TownHallReservation(code, label, "", "", 0, 0.0, "", 0, 0, TownHallReservationStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallReservationStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallReservationStatus next)
    {
        switch (Status)
        {
            case TownHallReservationStatus.New:
                switch (next)
                {
                    case TownHallReservationStatus.Queued:
                    case TownHallReservationStatus.Done:
                        return true;
                }
                return false;
            case TownHallReservationStatus.Queued:
                switch (next)
                {
                    case TownHallReservationStatus.Running:
                    case TownHallReservationStatus.Failed:
                        return true;
                }
                return false;
            case TownHallReservationStatus.Running:
                return next == TownHallReservationStatus.Done || next == TownHallReservationStatus.Failed;
            case TownHallReservationStatus.Done:
                switch (next)
                {
                    case TownHallReservationStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetTotal()
    {
        return Math.Round((double)BatchSize - (double)Retries, 2);
    }

    public TownHallReservation Copy()
    {
        return new TownHallReservation(
            Code,
            Label,
            Location,
            ContactEmail,
            Retries,
            Budget,
            Currency,
            CreatedAt,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
