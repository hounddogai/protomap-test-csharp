namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered mobility reservation is in its life cycle.</summary>
public enum TieredMobilityReservationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A tiered mobility reservation as the mobility team keeps it.</summary>
public sealed class TieredMobilityReservation(string code, string name, string currency, string ownerId, double maximum, string jobCode, double cost, double margin, string channel, TieredMobilityReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Currency { get; set; } = currency;
    public string OwnerId { get; set; } = ownerId;
    public double Maximum { get; set; } = maximum;
    public string JobCode { get; set; } = jobCode;
    public double Cost { get; set; } = cost;
    public double Margin { get; set; } = margin;
    public string Channel { get; set; } = channel;
    public TieredMobilityReservationStatus Status { get; set; } = status;

    /// <summary>Starts a tiered mobility reservation in status open.</summary>
    public static TieredMobilityReservation NewTieredMobilityReservation(string code, string name)
    {
        return new TieredMobilityReservation(
            code,
            name,
            "",
            "",
            0.0,
            "",
            0.0,
            0.0,
            "",
            TieredMobilityReservationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredMobilityReservationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredMobilityReservationStatus next)
    {
        switch (Status)
        {
            case TieredMobilityReservationStatus.Open:
                return next == TieredMobilityReservationStatus.Reopened;
            case TieredMobilityReservationStatus.InReview:
                return next == TieredMobilityReservationStatus.Resolved || next == TieredMobilityReservationStatus.Reopened;
            case TieredMobilityReservationStatus.Resolved:
                switch (next)
                {
                    case TieredMobilityReservationStatus.Reopened:
                    case TieredMobilityReservationStatus.Archived:
                        return true;
                }
                return false;
            case TieredMobilityReservationStatus.Reopened:
                return next == TieredMobilityReservationStatus.Archived;
        }
        return false;
    }

    public void MoveTo(TieredMobilityReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered mobility reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Maximum * Cost, 2);
    }

    public TieredMobilityReservation Copy()
    {
        return new TieredMobilityReservation(
            Code,
            Name,
            Currency,
            OwnerId,
            Maximum,
            JobCode,
            Cost,
            Margin,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
