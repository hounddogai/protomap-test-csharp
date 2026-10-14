namespace Acme.Workforce.Talent.Mobility.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility reservation is in its life cycle.</summary>
public enum MobilityReservationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A mobility reservation as the mobility team keeps it.</summary>
public sealed class MobilityReservation(string code, string label, string country, int windowDays, int batchSize, double discount, int capacity, double maximum, double fte, string grade, MobilityReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public int WindowDays { get; set; } = windowDays;
    public int BatchSize { get; set; } = batchSize;
    public double Discount { get; set; } = discount;
    public int Capacity { get; set; } = capacity;
    public double Maximum { get; set; } = maximum;
    public double Fte { get; set; } = fte;
    public string Grade { get; set; } = grade;
    public MobilityReservationStatus Status { get; set; } = status;

    /// <summary>Starts a mobility reservation in status open.</summary>
    public static MobilityReservation NewMobilityReservation(string code, string label)
    {
        return new MobilityReservation(code, label, "", 0, 0, 0.0, 0, 0.0, 0.0, "", MobilityReservationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityReservationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityReservationStatus next)
    {
        switch (Status)
        {
            case MobilityReservationStatus.Open:
                return next == MobilityReservationStatus.InReview;
            case MobilityReservationStatus.InReview:
                switch (next)
                {
                    case MobilityReservationStatus.Resolved:
                    case MobilityReservationStatus.Reopened:
                        return true;
                }
                return false;
            case MobilityReservationStatus.Resolved:
                return next == MobilityReservationStatus.Reopened;
            case MobilityReservationStatus.Reopened:
                return next == MobilityReservationStatus.Archived;
        }
        return false;
    }

    public void MoveTo(MobilityReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedVolume()
    {
        return Math.Round(Discount * Maximum, 2);
    }

    public MobilityReservation Copy()
    {
        return new MobilityReservation(
            Code,
            Label,
            Country,
            WindowDays,
            BatchSize,
            Discount,
            Capacity,
            Maximum,
            Fte,
            Grade,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
