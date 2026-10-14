namespace Acme.Workforce.Compensation.Benchmark.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark audit is in its life cycle.</summary>
public enum BenchmarkAuditStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A benchmark audit as the benchmark team keeps it.</summary>
public sealed class BenchmarkAudit(string code, string name, bool approved, int graceDays, double ratio, string ownerId, BenchmarkAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Approved { get; set; } = approved;
    public int GraceDays { get; set; } = graceDays;
    public double Ratio { get; set; } = ratio;
    public string OwnerId { get; set; } = ownerId;
    public BenchmarkAuditStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark audit in status new.</summary>
    public static BenchmarkAudit NewBenchmarkAudit(string code, string name)
    {
        return new BenchmarkAudit(code, name, false, 0, 0.0, "", BenchmarkAuditStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkAuditStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkAuditStatus next)
    {
        switch (Status)
        {
            case BenchmarkAuditStatus.New:
                return next == BenchmarkAuditStatus.Running || next == BenchmarkAuditStatus.Done;
            case BenchmarkAuditStatus.Queued:
                return next == BenchmarkAuditStatus.Done;
            case BenchmarkAuditStatus.Running:
                switch (next)
                {
                    case BenchmarkAuditStatus.Failed:
                        return true;
                }
                return false;
            case BenchmarkAuditStatus.Done:
                switch (next)
                {
                    case BenchmarkAuditStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BenchmarkAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)GraceDays * Ratio, 2);
    }

    public BenchmarkAudit Copy()
    {
        return new BenchmarkAudit(Code, Name, Approved, GraceDays, Ratio, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
