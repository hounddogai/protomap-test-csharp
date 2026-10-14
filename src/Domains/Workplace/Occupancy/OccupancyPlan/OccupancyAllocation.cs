namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy allocation is in its life cycle.</summary>
public enum OccupancyAllocationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a occupancy allocation and its current status.</summary>
public sealed class OccupancyAllocation(string code, string label, string country, string locale, int updatedAt, OccupancyAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public string Locale { get; set; } = locale;
    public int UpdatedAt { get; set; } = updatedAt;
    public OccupancyAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy allocation in status idle.</summary>
    public static OccupancyAllocation NewOccupancyAllocation(string code, string label)
    {
        return new OccupancyAllocation(code, label, "", "", 0, OccupancyAllocationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyAllocationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyAllocationStatus next)
    {
        switch (Status)
        {
            case OccupancyAllocationStatus.Idle:
                switch (next)
                {
                    case OccupancyAllocationStatus.Warming:
                    case OccupancyAllocationStatus.Ready:
                        return true;
                }
                return false;
            case OccupancyAllocationStatus.Warming:
                switch (next)
                {
                    case OccupancyAllocationStatus.Draining:
                    case OccupancyAllocationStatus.Stopped:
                        return true;
                }
                return false;
            case OccupancyAllocationStatus.Ready:
                return next == OccupancyAllocationStatus.Draining || next == OccupancyAllocationStatus.Stopped;
            case OccupancyAllocationStatus.Draining:
                switch (next)
                {
                    case OccupancyAllocationStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"occupancy allocation"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)UpdatedAt;
    }

    public OccupancyAllocation Copy()
    {
        return new OccupancyAllocation(Code, Label, Country, Locale, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
