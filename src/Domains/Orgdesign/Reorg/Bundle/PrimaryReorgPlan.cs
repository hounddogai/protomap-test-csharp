namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary reorg plan is in its life cycle.</summary>
public enum PrimaryReorgPlanStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A primary reorg plan as the reorg team keeps it.</summary>
public sealed class PrimaryReorgPlan(string code, string label, string title, string location, double cost, string badge, int startsAt, PrimaryReorgPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public double Cost { get; set; } = cost;
    public string Badge { get; set; } = badge;
    public int StartsAt { get; set; } = startsAt;
    public PrimaryReorgPlanStatus Status { get; set; } = status;

    /// <summary>Starts a primary reorg plan in status requested.</summary>
    public static PrimaryReorgPlan NewPrimaryReorgPlan(string code, string label)
    {
        return new PrimaryReorgPlan(code, label, "", "", 0.0, "", 0, PrimaryReorgPlanStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryReorgPlanStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryReorgPlanStatus next)
    {
        switch (Status)
        {
            case PrimaryReorgPlanStatus.Requested:
                return next == PrimaryReorgPlanStatus.Granted;
            case PrimaryReorgPlanStatus.Granted:
                return next == PrimaryReorgPlanStatus.Revoked || next == PrimaryReorgPlanStatus.Expired;
            case PrimaryReorgPlanStatus.Revoked:
                switch (next)
                {
                    case PrimaryReorgPlanStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryReorgPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary reorg plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetExposure()
    {
        return Math.Round((double)StartsAt + Cost, 2);
    }

    public PrimaryReorgPlan Copy()
    {
        return new PrimaryReorgPlan(Code, Label, Title, Location, Cost, Badge, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
