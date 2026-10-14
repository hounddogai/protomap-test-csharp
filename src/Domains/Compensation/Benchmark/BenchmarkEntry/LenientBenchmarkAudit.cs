namespace Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient benchmark audit is in its life cycle.</summary>
public enum LenientBenchmarkAuditStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A lenient benchmark audit. Its code is unique within a tenant.</summary>
public sealed class LenientBenchmarkAudit(string code, string title, List<string> tags, string employeeId, double minimum, string managerId, int sequence, double cost, string phone, int limit, string locale, LenientBenchmarkAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public string EmployeeId { get; set; } = employeeId;
    public double Minimum { get; set; } = minimum;
    public string ManagerId { get; set; } = managerId;
    public int Sequence { get; set; } = sequence;
    public double Cost { get; set; } = cost;
    public string Phone { get; set; } = phone;
    public int Limit { get; set; } = limit;
    public string Locale { get; set; } = locale;
    public LenientBenchmarkAuditStatus Status { get; set; } = status;

    /// <summary>Starts a lenient benchmark audit in status created.</summary>
    public static LenientBenchmarkAudit NewLenientBenchmarkAudit(string code, string title)
    {
        return new LenientBenchmarkAudit(
            code,
            title,
            new List<string>(),
            "",
            0.0,
            "",
            0,
            0.0,
            "",
            0,
            "",
            LenientBenchmarkAuditStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientBenchmarkAuditStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientBenchmarkAuditStatus next)
    {
        switch (Status)
        {
            case LenientBenchmarkAuditStatus.Created:
                switch (next)
                {
                    case LenientBenchmarkAuditStatus.Validated:
                    case LenientBenchmarkAuditStatus.Retired:
                        return true;
                }
                return false;
            case LenientBenchmarkAuditStatus.Validated:
                switch (next)
                {
                    case LenientBenchmarkAuditStatus.Retired:
                        return true;
                }
                return false;
            case LenientBenchmarkAuditStatus.Published:
                switch (next)
                {
                    case LenientBenchmarkAuditStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientBenchmarkAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient benchmark audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient benchmark audit"} {Code}";
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

    public double ProjectedTotal()
    {
        return Math.Round(Cost * Minimum, 2);
    }

    public LenientBenchmarkAudit Copy()
    {
        return new LenientBenchmarkAudit(
            Code,
            Title,
            Tags,
            EmployeeId,
            Minimum,
            ManagerId,
            Sequence,
            Cost,
            Phone,
            Limit,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
