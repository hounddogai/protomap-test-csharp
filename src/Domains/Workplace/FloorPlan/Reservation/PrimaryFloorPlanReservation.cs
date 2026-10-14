namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary floor plan reservation is in its life cycle.</summary>
public enum PrimaryFloorPlanReservationStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a primary floor plan reservation and its current status.</summary>
public sealed class PrimaryFloorPlanReservation(string code, string name, double rating, double ceiling, string phone, double margin, double threshold, bool archived, double maximum, string locale, PrimaryFloorPlanReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rating { get; set; } = rating;
    public double Ceiling { get; set; } = ceiling;
    public string Phone { get; set; } = phone;
    public double Margin { get; set; } = margin;
    public double Threshold { get; set; } = threshold;
    public bool Archived { get; set; } = archived;
    public double Maximum { get; set; } = maximum;
    public string Locale { get; set; } = locale;
    public PrimaryFloorPlanReservationStatus Status { get; set; } = status;

    /// <summary>Starts a primary floor plan reservation in status new.</summary>
    public static PrimaryFloorPlanReservation NewPrimaryFloorPlanReservation(string code, string name)
    {
        return new PrimaryFloorPlanReservation(
            code,
            name,
            0.0,
            0.0,
            "",
            0.0,
            0.0,
            false,
            0.0,
            "",
            PrimaryFloorPlanReservationStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryFloorPlanReservationStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryFloorPlanReservationStatus next)
    {
        switch (Status)
        {
            case PrimaryFloorPlanReservationStatus.New:
                return next == PrimaryFloorPlanReservationStatus.Done;
            case PrimaryFloorPlanReservationStatus.Queued:
                return next == PrimaryFloorPlanReservationStatus.Done || next == PrimaryFloorPlanReservationStatus.Failed;
            case PrimaryFloorPlanReservationStatus.Running:
                return next == PrimaryFloorPlanReservationStatus.Failed;
            case PrimaryFloorPlanReservationStatus.Done:
                return next == PrimaryFloorPlanReservationStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PrimaryFloorPlanReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary floor plan reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"primary floor plan reservation"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Margin * Maximum, 2);
    }

    public PrimaryFloorPlanReservation Copy()
    {
        return new PrimaryFloorPlanReservation(
            Code,
            Name,
            Rating,
            Ceiling,
            Phone,
            Margin,
            Threshold,
            Archived,
            Maximum,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
