namespace Acme.Workforce.Compensation.Benchmark.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark window is in its life cycle.</summary>
public enum BenchmarkWindowStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A benchmark window. Its code is unique within a tenant.</summary>
public sealed class BenchmarkWindow(string code, string name, int version, bool active, string title, string currency, double rate, double fte, BenchmarkWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public bool Active { get; set; } = active;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public double Rate { get; set; } = rate;
    public double Fte { get; set; } = fte;
    public BenchmarkWindowStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark window in status proposed.</summary>
    public static BenchmarkWindow NewBenchmarkWindow(string code, string name)
    {
        return new BenchmarkWindow(code, name, 0, false, "", "", 0.0, 0.0, BenchmarkWindowStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkWindowStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkWindowStatus next)
    {
        switch (Status)
        {
            case BenchmarkWindowStatus.Proposed:
                switch (next)
                {
                    case BenchmarkWindowStatus.Rejected:
                    case BenchmarkWindowStatus.Withdrawn:
                        return true;
                }
                return false;
            case BenchmarkWindowStatus.Approved:
                return next == BenchmarkWindowStatus.Rejected || next == BenchmarkWindowStatus.Withdrawn;
            case BenchmarkWindowStatus.Rejected:
                switch (next)
                {
                    case BenchmarkWindowStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BenchmarkWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Version - Rate, 2);
    }

    public BenchmarkWindow Copy()
    {
        return new BenchmarkWindow(Code, Name, Version, Active, Title, Currency, Rate, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
