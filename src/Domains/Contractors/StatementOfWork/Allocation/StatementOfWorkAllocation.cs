namespace Acme.Workforce.Contractors.StatementOfWork.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work allocation is in its life cycle.</summary>
public enum StatementOfWorkAllocationStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A statement of work allocation as the statement of work team keeps it.</summary>
public sealed class StatementOfWorkAllocation(string code, string name, double ceiling, int revision, int capacity, double rating, bool pinned, int expiresAt, StatementOfWorkAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public int Revision { get; set; } = revision;
    public int Capacity { get; set; } = capacity;
    public double Rating { get; set; } = rating;
    public bool Pinned { get; set; } = pinned;
    public int ExpiresAt { get; set; } = expiresAt;
    public StatementOfWorkAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work allocation in status draft.</summary>
    public static StatementOfWorkAllocation NewStatementOfWorkAllocation(string code, string name)
    {
        return new StatementOfWorkAllocation(
            code,
            name,
            0.0,
            0,
            0,
            0.0,
            false,
            0,
            StatementOfWorkAllocationStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkAllocationStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkAllocationStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkAllocationStatus.Draft:
                switch (next)
                {
                    case StatementOfWorkAllocationStatus.Pending:
                    case StatementOfWorkAllocationStatus.Closed:
                        return true;
                }
                return false;
            case StatementOfWorkAllocationStatus.Pending:
                return next == StatementOfWorkAllocationStatus.Active || next == StatementOfWorkAllocationStatus.Closed;
            case StatementOfWorkAllocationStatus.Active:
                return next == StatementOfWorkAllocationStatus.Suspended;
            case StatementOfWorkAllocationStatus.Suspended:
                switch (next)
                {
                    case StatementOfWorkAllocationStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetExposure()
    {
        return Math.Round(Ceiling * Rating, 2);
    }

    public StatementOfWorkAllocation Copy()
    {
        return new StatementOfWorkAllocation(
            Code,
            Name,
            Ceiling,
            Revision,
            Capacity,
            Rating,
            Pinned,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
