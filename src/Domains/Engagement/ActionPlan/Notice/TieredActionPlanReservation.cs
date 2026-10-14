namespace Acme.Workforce.Engagement.ActionPlan.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered action plan reservation is in its life cycle.</summary>
public enum TieredActionPlanReservationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A tiered action plan reservation as the action plan team keeps it.</summary>
public sealed class TieredActionPlanReservation(string code, string name, double score, int retries, string badge, double weight, string team, string location, int batchSize, string externalRef, int capacity, TieredActionPlanReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public int Retries { get; set; } = retries;
    public string Badge { get; set; } = badge;
    public double Weight { get; set; } = weight;
    public string Team { get; set; } = team;
    public string Location { get; set; } = location;
    public int BatchSize { get; set; } = batchSize;
    public string ExternalRef { get; set; } = externalRef;
    public int Capacity { get; set; } = capacity;
    public TieredActionPlanReservationStatus Status { get; set; } = status;

    /// <summary>Starts a tiered action plan reservation in status proposed.</summary>
    public static TieredActionPlanReservation NewTieredActionPlanReservation(string code, string name)
    {
        return new TieredActionPlanReservation(
            code,
            name,
            0.0,
            0,
            "",
            0.0,
            "",
            "",
            0,
            "",
            0,
            TieredActionPlanReservationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredActionPlanReservationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredActionPlanReservationStatus next)
    {
        switch (Status)
        {
            case TieredActionPlanReservationStatus.Proposed:
                return next == TieredActionPlanReservationStatus.Approved;
            case TieredActionPlanReservationStatus.Approved:
                return next == TieredActionPlanReservationStatus.Rejected || next == TieredActionPlanReservationStatus.Withdrawn;
            case TieredActionPlanReservationStatus.Rejected:
                return next == TieredActionPlanReservationStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(TieredActionPlanReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered action plan reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"tiered action plan reservation"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Capacity + (double)Retries, 2);
    }

    public TieredActionPlanReservation Copy()
    {
        return new TieredActionPlanReservation(
            Code,
            Name,
            Score,
            Retries,
            Badge,
            Weight,
            Team,
            Location,
            BatchSize,
            ExternalRef,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
