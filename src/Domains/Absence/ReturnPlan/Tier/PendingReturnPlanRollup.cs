namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending return plan rollup is in its life cycle.</summary>
public enum PendingReturnPlanRollupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a pending return plan rollup and its current status.</summary>
public sealed class PendingReturnPlanRollup(string code, string title, int capacity, double cost, int quantity, double weight, double minimum, double floorValue, PendingReturnPlanRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Capacity { get; set; } = capacity;
    public double Cost { get; set; } = cost;
    public int Quantity { get; set; } = quantity;
    public double Weight { get; set; } = weight;
    public double Minimum { get; set; } = minimum;
    public double FloorValue { get; set; } = floorValue;
    public PendingReturnPlanRollupStatus Status { get; set; } = status;

    /// <summary>Starts a pending return plan rollup in status created.</summary>
    public static PendingReturnPlanRollup NewPendingReturnPlanRollup(string code, string title)
    {
        return new PendingReturnPlanRollup(
            code,
            title,
            0,
            0.0,
            0,
            0.0,
            0.0,
            0.0,
            PendingReturnPlanRollupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingReturnPlanRollupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingReturnPlanRollupStatus next)
    {
        switch (Status)
        {
            case PendingReturnPlanRollupStatus.Created:
                return next == PendingReturnPlanRollupStatus.Retired;
            case PendingReturnPlanRollupStatus.Validated:
                switch (next)
                {
                    case PendingReturnPlanRollupStatus.Retired:
                        return true;
                }
                return false;
            case PendingReturnPlanRollupStatus.Published:
                return next == PendingReturnPlanRollupStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PendingReturnPlanRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending return plan rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveValue()
    {
        return Math.Round(Cost - Minimum, 2);
    }

    public PendingReturnPlanRollup Copy()
    {
        return new PendingReturnPlanRollup(
            Code,
            Title,
            Capacity,
            Cost,
            Quantity,
            Weight,
            Minimum,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
