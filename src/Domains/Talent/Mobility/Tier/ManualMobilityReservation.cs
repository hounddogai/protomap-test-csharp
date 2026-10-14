namespace Acme.Workforce.Talent.Mobility.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual mobility reservation is in its life cycle.</summary>
public enum ManualMobilityReservationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One manual mobility reservation, with what the planners need to act on it.</summary>
public sealed class ManualMobilityReservation(string code, string title, string grade, double maximum, string segment, string currency, double margin, double discount, string managerId, ManualMobilityReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public double Maximum { get; set; } = maximum;
    public string Segment { get; set; } = segment;
    public string Currency { get; set; } = currency;
    public double Margin { get; set; } = margin;
    public double Discount { get; set; } = discount;
    public string ManagerId { get; set; } = managerId;
    public ManualMobilityReservationStatus Status { get; set; } = status;

    /// <summary>Starts a manual mobility reservation in status idle.</summary>
    public static ManualMobilityReservation NewManualMobilityReservation(string code, string title)
    {
        return new ManualMobilityReservation(
            code,
            title,
            "",
            0.0,
            "",
            "",
            0.0,
            0.0,
            "",
            ManualMobilityReservationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualMobilityReservationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualMobilityReservationStatus next)
    {
        switch (Status)
        {
            case ManualMobilityReservationStatus.Idle:
                switch (next)
                {
                    case ManualMobilityReservationStatus.Ready:
                        return true;
                }
                return false;
            case ManualMobilityReservationStatus.Warming:
                return next == ManualMobilityReservationStatus.Stopped;
            case ManualMobilityReservationStatus.Ready:
                return next == ManualMobilityReservationStatus.Stopped;
            case ManualMobilityReservationStatus.Draining:
                switch (next)
                {
                    case ManualMobilityReservationStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualMobilityReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual mobility reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Margin + Discount, 2);
    }

    public ManualMobilityReservation Copy()
    {
        return new ManualMobilityReservation(
            Code,
            Title,
            Grade,
            Maximum,
            Segment,
            Currency,
            Margin,
            Discount,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
