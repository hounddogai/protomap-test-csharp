namespace Acme.Workforce.Workplace.Occupancy.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered occupancy allocation is in its life cycle.</summary>
public enum TieredOccupancyAllocationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A tiered occupancy allocation. Its code is unique within a tenant.</summary>
public sealed class TieredOccupancyAllocation(string code, string name, string contactEmail, string managerId, string badge, int quantity, TieredOccupancyAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public string ManagerId { get; set; } = managerId;
    public string Badge { get; set; } = badge;
    public int Quantity { get; set; } = quantity;
    public TieredOccupancyAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a tiered occupancy allocation in status requested.</summary>
    public static TieredOccupancyAllocation NewTieredOccupancyAllocation(string code, string name)
    {
        return new TieredOccupancyAllocation(code, name, "", "", "", 0, TieredOccupancyAllocationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredOccupancyAllocationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredOccupancyAllocationStatus next)
    {
        switch (Status)
        {
            case TieredOccupancyAllocationStatus.Requested:
                switch (next)
                {
                    case TieredOccupancyAllocationStatus.Granted:
                        return true;
                }
                return false;
            case TieredOccupancyAllocationStatus.Granted:
                switch (next)
                {
                    case TieredOccupancyAllocationStatus.Revoked:
                        return true;
                }
                return false;
            case TieredOccupancyAllocationStatus.Revoked:
                return next == TieredOccupancyAllocationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TieredOccupancyAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered occupancy allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"tiered occupancy allocation"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public TieredOccupancyAllocation Copy()
    {
        return new TieredOccupancyAllocation(Code, Name, ContactEmail, ManagerId, Badge, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
