namespace Acme.Workforce.Contractors.Rate.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled rate allocation is in its life cycle.</summary>
public enum ScheduledRateAllocationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A scheduled rate allocation as the rate team keeps it.</summary>
public sealed class ScheduledRateAllocation(string code, string label, double amount, string department, int priority, int expiresAt, ScheduledRateAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public string Department { get; set; } = department;
    public int Priority { get; set; } = priority;
    public int ExpiresAt { get; set; } = expiresAt;
    public ScheduledRateAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled rate allocation in status created.</summary>
    public static ScheduledRateAllocation NewScheduledRateAllocation(string code, string label)
    {
        return new ScheduledRateAllocation(code, label, 0.0, "", 0, 0, ScheduledRateAllocationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledRateAllocationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledRateAllocationStatus next)
    {
        switch (Status)
        {
            case ScheduledRateAllocationStatus.Created:
                switch (next)
                {
                    case ScheduledRateAllocationStatus.Published:
                        return true;
                }
                return false;
            case ScheduledRateAllocationStatus.Validated:
                return next == ScheduledRateAllocationStatus.Published || next == ScheduledRateAllocationStatus.Retired;
            case ScheduledRateAllocationStatus.Published:
                switch (next)
                {
                    case ScheduledRateAllocationStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledRateAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled rate allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"scheduled rate allocation"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Priority - Amount, 2);
    }

    public ScheduledRateAllocation Copy()
    {
        return new ScheduledRateAllocation(Code, Label, Amount, Department, Priority, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
