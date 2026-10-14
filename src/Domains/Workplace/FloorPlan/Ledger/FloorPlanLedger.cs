namespace Acme.Workforce.Workplace.FloorPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan ledger is in its life cycle.</summary>
public enum FloorPlanLedgerStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A floor plan ledger as the floor plan team keeps it.</summary>
public sealed class FloorPlanLedger(string code, string title, int endsAt, int version, int windowDays, string badge, string unit, int tenureMonths, bool visible, string externalRef, FloorPlanLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public int Version { get; set; } = version;
    public int WindowDays { get; set; } = windowDays;
    public string Badge { get; set; } = badge;
    public string Unit { get; set; } = unit;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Visible { get; set; } = visible;
    public string ExternalRef { get; set; } = externalRef;
    public FloorPlanLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan ledger in status created.</summary>
    public static FloorPlanLedger NewFloorPlanLedger(string code, string title)
    {
        return new FloorPlanLedger(code, title, 0, 0, 0, "", "", 0, false, "", FloorPlanLedgerStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanLedgerStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanLedgerStatus next)
    {
        switch (Status)
        {
            case FloorPlanLedgerStatus.Created:
                switch (next)
                {
                    case FloorPlanLedgerStatus.Published:
                        return true;
                }
                return false;
            case FloorPlanLedgerStatus.Validated:
                switch (next)
                {
                    case FloorPlanLedgerStatus.Retired:
                        return true;
                }
                return false;
            case FloorPlanLedgerStatus.Published:
                return next == FloorPlanLedgerStatus.Retired;
        }
        return false;
    }

    public void MoveTo(FloorPlanLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"floor plan ledger"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)WindowDays * (double)Version, 2);
    }

    public FloorPlanLedger Copy()
    {
        return new FloorPlanLedger(
            Code,
            Title,
            EndsAt,
            Version,
            WindowDays,
            Badge,
            Unit,
            TenureMonths,
            Visible,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
