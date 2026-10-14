namespace Acme.Workforce.Orgdesign.Span.SpanReview;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span cycle is in its life cycle.</summary>
public enum SpanCycleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A span cycle. Its code is unique within a tenant.</summary>
public sealed class SpanCycle(string code, string title, string timezone, string department, string employeeId, string salaryBand, double hours, int retries, int batchSize, List<string> tags, string unit, SpanCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public string Department { get; set; } = department;
    public string EmployeeId { get; set; } = employeeId;
    public string SalaryBand { get; set; } = salaryBand;
    public double Hours { get; set; } = hours;
    public int Retries { get; set; } = retries;
    public int BatchSize { get; set; } = batchSize;
    public List<string> Tags { get; set; } = tags;
    public string Unit { get; set; } = unit;
    public SpanCycleStatus Status { get; set; } = status;

    /// <summary>Starts a span cycle in status planned.</summary>
    public static SpanCycle NewSpanCycle(string code, string title)
    {
        return new SpanCycle(
            code,
            title,
            "",
            "",
            "",
            "",
            0.0,
            0,
            0,
            new List<string>(),
            "",
            SpanCycleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanCycleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanCycleStatus next)
    {
        switch (Status)
        {
            case SpanCycleStatus.Planned:
                switch (next)
                {
                    case SpanCycleStatus.InProgress:
                    case SpanCycleStatus.Completed:
                        return true;
                }
                return false;
            case SpanCycleStatus.Scheduled:
                return next == SpanCycleStatus.Completed || next == SpanCycleStatus.Cancelled;
            case SpanCycleStatus.InProgress:
                switch (next)
                {
                    case SpanCycleStatus.Completed:
                    case SpanCycleStatus.Cancelled:
                        return true;
                }
                return false;
            case SpanCycleStatus.Completed:
                return next == SpanCycleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SpanCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedValue()
    {
        return Math.Round((double)Retries * Hours, 2);
    }

    public SpanCycle Copy()
    {
        return new SpanCycle(
            Code,
            Title,
            Timezone,
            Department,
            EmployeeId,
            SalaryBand,
            Hours,
            Retries,
            BatchSize,
            Tags,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
