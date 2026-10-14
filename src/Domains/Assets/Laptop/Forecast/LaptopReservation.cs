namespace Acme.Workforce.Assets.Laptop.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop reservation is in its life cycle.</summary>
public enum LaptopReservationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A laptop reservation as the laptop team keeps it.</summary>
public sealed class LaptopReservation(string code, string label, string title, double score, string department, double ceiling, int sequence, string currency, string channel, double amount, string checksum, LaptopReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public string Department { get; set; } = department;
    public double Ceiling { get; set; } = ceiling;
    public int Sequence { get; set; } = sequence;
    public string Currency { get; set; } = currency;
    public string Channel { get; set; } = channel;
    public double Amount { get; set; } = amount;
    public string Checksum { get; set; } = checksum;
    public LaptopReservationStatus Status { get; set; } = status;

    /// <summary>Starts a laptop reservation in status proposed.</summary>
    public static LaptopReservation NewLaptopReservation(string code, string label)
    {
        return new LaptopReservation(
            code,
            label,
            "",
            0.0,
            "",
            0.0,
            0,
            "",
            "",
            0.0,
            "",
            LaptopReservationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopReservationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopReservationStatus next)
    {
        switch (Status)
        {
            case LaptopReservationStatus.Proposed:
                return next == LaptopReservationStatus.Withdrawn;
            case LaptopReservationStatus.Approved:
                return next == LaptopReservationStatus.Rejected;
            case LaptopReservationStatus.Rejected:
                switch (next)
                {
                    case LaptopReservationStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LaptopReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetTotal()
    {
        return Math.Round((double)Sequence + Score, 2);
    }

    public LaptopReservation Copy()
    {
        return new LaptopReservation(
            Code,
            Label,
            Title,
            Score,
            Department,
            Ceiling,
            Sequence,
            Currency,
            Channel,
            Amount,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
