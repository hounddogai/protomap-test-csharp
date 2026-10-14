namespace Acme.Workforce.Absence.Accrual.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual allocation is in its life cycle.</summary>
public enum AccrualAllocationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a accrual allocation and its current status.</summary>
public sealed class AccrualAllocation(string code, string name, string unit, string location, string managerId, string badge, string salaryBand, string ownerId, Dictionary<string, string> attributes, double amount, AccrualAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public string Location { get; set; } = location;
    public string ManagerId { get; set; } = managerId;
    public string Badge { get; set; } = badge;
    public string SalaryBand { get; set; } = salaryBand;
    public string OwnerId { get; set; } = ownerId;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Amount { get; set; } = amount;
    public AccrualAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a accrual allocation in status requested.</summary>
    public static AccrualAllocation NewAccrualAllocation(string code, string name)
    {
        return new AccrualAllocation(
            code,
            name,
            "",
            "",
            "",
            "",
            "",
            "",
            new Dictionary<string, string>(),
            0.0,
            AccrualAllocationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualAllocationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualAllocationStatus next)
    {
        switch (Status)
        {
            case AccrualAllocationStatus.Requested:
                switch (next)
                {
                    case AccrualAllocationStatus.Granted:
                        return true;
                }
                return false;
            case AccrualAllocationStatus.Granted:
                switch (next)
                {
                    case AccrualAllocationStatus.Revoked:
                    case AccrualAllocationStatus.Expired:
                        return true;
                }
                return false;
            case AccrualAllocationStatus.Revoked:
                return next == AccrualAllocationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AccrualAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"accrual allocation"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public AccrualAllocation Copy()
    {
        return new AccrualAllocation(
            Code,
            Name,
            Unit,
            Location,
            ManagerId,
            Badge,
            SalaryBand,
            OwnerId,
            Attributes,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
