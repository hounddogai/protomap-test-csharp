namespace Acme.Workforce.Talent.Pool.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool entry is in its life cycle.</summary>
public enum PoolEntryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pool entry. Its code is unique within a tenant.</summary>
public sealed class PoolEntry(string code, string name, int capacity, int priority, string timezone, double discount, double amount, int createdAt, string department, int batchSize, string reviewer, PoolEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public int Priority { get; set; } = priority;
    public string Timezone { get; set; } = timezone;
    public double Discount { get; set; } = discount;
    public double Amount { get; set; } = amount;
    public int CreatedAt { get; set; } = createdAt;
    public string Department { get; set; } = department;
    public int BatchSize { get; set; } = batchSize;
    public string Reviewer { get; set; } = reviewer;
    public PoolEntryStatus Status { get; set; } = status;

    /// <summary>Starts a pool entry in status proposed.</summary>
    public static PoolEntry NewPoolEntry(string code, string name)
    {
        return new PoolEntry(code, name, 0, 0, "", 0.0, 0.0, 0, "", 0, "", PoolEntryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolEntryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolEntryStatus next)
    {
        switch (Status)
        {
            case PoolEntryStatus.Proposed:
                switch (next)
                {
                    case PoolEntryStatus.Rejected:
                        return true;
                }
                return false;
            case PoolEntryStatus.Approved:
                switch (next)
                {
                    case PoolEntryStatus.Rejected:
                    case PoolEntryStatus.Withdrawn:
                        return true;
                }
                return false;
            case PoolEntryStatus.Rejected:
                switch (next)
                {
                    case PoolEntryStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"pool entry"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)CreatedAt - Discount, 2);
    }

    public PoolEntry Copy()
    {
        return new PoolEntry(
            Code,
            Name,
            Capacity,
            Priority,
            Timezone,
            Discount,
            Amount,
            CreatedAt,
            Department,
            BatchSize,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
