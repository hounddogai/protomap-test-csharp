namespace Acme.Workforce.Workplace.Occupancy.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy adjustment is in its life cycle.</summary>
public enum OccupancyAdjustmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One occupancy adjustment, with what the planners need to act on it.</summary>
public sealed class OccupancyAdjustment(string code, string title, string ownerId, bool active, double cost, string externalRef, OccupancyAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public bool Active { get; set; } = active;
    public double Cost { get; set; } = cost;
    public string ExternalRef { get; set; } = externalRef;
    public OccupancyAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy adjustment in status draft.</summary>
    public static OccupancyAdjustment NewOccupancyAdjustment(string code, string title)
    {
        return new OccupancyAdjustment(code, title, "", false, 0.0, "", OccupancyAdjustmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyAdjustmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyAdjustmentStatus next)
    {
        switch (Status)
        {
            case OccupancyAdjustmentStatus.Draft:
                switch (next)
                {
                    case OccupancyAdjustmentStatus.Closed:
                        return true;
                }
                return false;
            case OccupancyAdjustmentStatus.Pending:
                return next == OccupancyAdjustmentStatus.Suspended;
            case OccupancyAdjustmentStatus.Active:
                switch (next)
                {
                    case OccupancyAdjustmentStatus.Suspended:
                        return true;
                }
                return false;
            case OccupancyAdjustmentStatus.Suspended:
                return next == OccupancyAdjustmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(OccupancyAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Cost;
    }

    public OccupancyAdjustment Copy()
    {
        return new OccupancyAdjustment(Code, Title, OwnerId, Active, Cost, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
