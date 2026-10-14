namespace Acme.Workforce.Contractors.StatementOfWork.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work adjustment is in its life cycle.</summary>
public enum StatementOfWorkAdjustmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A statement of work adjustment. Its code is unique within a tenant.</summary>
public sealed class StatementOfWorkAdjustment(string code, string title, string department, int createdAt, int limit, double amount, string salaryBand, string currency, string summary, Dictionary<string, string> attributes, int capacity, StatementOfWorkAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Department { get; set; } = department;
    public int CreatedAt { get; set; } = createdAt;
    public int Limit { get; set; } = limit;
    public double Amount { get; set; } = amount;
    public string SalaryBand { get; set; } = salaryBand;
    public string Currency { get; set; } = currency;
    public string Summary { get; set; } = summary;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Capacity { get; set; } = capacity;
    public StatementOfWorkAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work adjustment in status planned.</summary>
    public static StatementOfWorkAdjustment NewStatementOfWorkAdjustment(string code, string title)
    {
        return new StatementOfWorkAdjustment(
            code,
            title,
            "",
            0,
            0,
            0.0,
            "",
            "",
            "",
            new Dictionary<string, string>(),
            0,
            StatementOfWorkAdjustmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkAdjustmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkAdjustmentStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkAdjustmentStatus.Planned:
                return next == StatementOfWorkAdjustmentStatus.Scheduled;
            case StatementOfWorkAdjustmentStatus.Scheduled:
                return next == StatementOfWorkAdjustmentStatus.Cancelled;
            case StatementOfWorkAdjustmentStatus.InProgress:
                switch (next)
                {
                    case StatementOfWorkAdjustmentStatus.Completed:
                    case StatementOfWorkAdjustmentStatus.Cancelled:
                        return true;
                }
                return false;
            case StatementOfWorkAdjustmentStatus.Completed:
                switch (next)
                {
                    case StatementOfWorkAdjustmentStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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

    public double NetExposure()
    {
        return Math.Round((double)Limit * (double)CreatedAt, 2);
    }

    public StatementOfWorkAdjustment Copy()
    {
        return new StatementOfWorkAdjustment(
            Code,
            Title,
            Department,
            CreatedAt,
            Limit,
            Amount,
            SalaryBand,
            Currency,
            Summary,
            Attributes,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
