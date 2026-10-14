namespace Acme.Workforce.Talent.SuccessionPlan.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan reservation is in its life cycle.</summary>
public enum SuccessionPlanReservationStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A succession plan reservation. Its code is unique within a tenant.</summary>
public sealed class SuccessionPlanReservation(string code, string name, double maximum, string team, string contactEmail, SuccessionPlanReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public string Team { get; set; } = team;
    public string ContactEmail { get; set; } = contactEmail;
    public SuccessionPlanReservationStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan reservation in status new.</summary>
    public static SuccessionPlanReservation NewSuccessionPlanReservation(string code, string name)
    {
        return new SuccessionPlanReservation(code, name, 0.0, "", "", SuccessionPlanReservationStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanReservationStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanReservationStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanReservationStatus.New:
                return next == SuccessionPlanReservationStatus.Running;
            case SuccessionPlanReservationStatus.Queued:
                return next == SuccessionPlanReservationStatus.Running || next == SuccessionPlanReservationStatus.Failed;
            case SuccessionPlanReservationStatus.Running:
                return next == SuccessionPlanReservationStatus.Failed;
            case SuccessionPlanReservationStatus.Done:
                return next == SuccessionPlanReservationStatus.Failed;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public SuccessionPlanReservation Copy()
    {
        return new SuccessionPlanReservation(Code, Name, Maximum, Team, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
