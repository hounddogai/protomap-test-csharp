namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan reservation is in its life cycle.</summary>
public enum FloorPlanReservationStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a floor plan reservation and its current status.</summary>
public sealed class FloorPlanReservation(string code, string label, string contactEmail, int expiresAt, string description, FloorPlanReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ContactEmail { get; set; } = contactEmail;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Description { get; set; } = description;
    public FloorPlanReservationStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan reservation in status new.</summary>
    public static FloorPlanReservation NewFloorPlanReservation(string code, string label)
    {
        return new FloorPlanReservation(code, label, "", 0, "", FloorPlanReservationStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanReservationStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanReservationStatus next)
    {
        switch (Status)
        {
            case FloorPlanReservationStatus.New:
                return next == FloorPlanReservationStatus.Failed;
            case FloorPlanReservationStatus.Queued:
                switch (next)
                {
                    case FloorPlanReservationStatus.Running:
                    case FloorPlanReservationStatus.Failed:
                        return true;
                }
                return false;
            case FloorPlanReservationStatus.Running:
                return next == FloorPlanReservationStatus.Done || next == FloorPlanReservationStatus.Failed;
            case FloorPlanReservationStatus.Done:
                return next == FloorPlanReservationStatus.Failed;
        }
        return false;
    }

    public void MoveTo(FloorPlanReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public FloorPlanReservation Copy()
    {
        return new FloorPlanReservation(Code, Label, ContactEmail, ExpiresAt, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
