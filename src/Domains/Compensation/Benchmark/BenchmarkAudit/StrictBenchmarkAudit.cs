namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict benchmark audit is in its life cycle.</summary>
public enum StrictBenchmarkAuditStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A strict benchmark audit as the benchmark team keeps it.</summary>
public sealed class StrictBenchmarkAudit(string code, string label, int priority, int createdAt, string phone, double rate, int quantity, StrictBenchmarkAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public int CreatedAt { get; set; } = createdAt;
    public string Phone { get; set; } = phone;
    public double Rate { get; set; } = rate;
    public int Quantity { get; set; } = quantity;
    public StrictBenchmarkAuditStatus Status { get; set; } = status;

    /// <summary>Starts a strict benchmark audit in status draft.</summary>
    public static StrictBenchmarkAudit NewStrictBenchmarkAudit(string code, string label)
    {
        return new StrictBenchmarkAudit(code, label, 0, 0, "", 0.0, 0, StrictBenchmarkAuditStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictBenchmarkAuditStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictBenchmarkAuditStatus next)
    {
        switch (Status)
        {
            case StrictBenchmarkAuditStatus.Draft:
                switch (next)
                {
                    case StrictBenchmarkAuditStatus.Pending:
                    case StrictBenchmarkAuditStatus.Suspended:
                        return true;
                }
                return false;
            case StrictBenchmarkAuditStatus.Pending:
                return next == StrictBenchmarkAuditStatus.Active || next == StrictBenchmarkAuditStatus.Closed;
            case StrictBenchmarkAuditStatus.Active:
                return next == StrictBenchmarkAuditStatus.Suspended;
            case StrictBenchmarkAuditStatus.Suspended:
                return next == StrictBenchmarkAuditStatus.Closed;
        }
        return false;
    }

    public void MoveTo(StrictBenchmarkAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict benchmark audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)CreatedAt - (double)Priority, 2);
    }

    public StrictBenchmarkAudit Copy()
    {
        return new StrictBenchmarkAudit(Code, Label, Priority, CreatedAt, Phone, Rate, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
