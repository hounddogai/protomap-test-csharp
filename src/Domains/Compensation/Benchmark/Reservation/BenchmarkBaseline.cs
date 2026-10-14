namespace Acme.Workforce.Compensation.Benchmark.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark baseline is in its life cycle.</summary>
public enum BenchmarkBaselineStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A benchmark baseline as the benchmark team keeps it.</summary>
public sealed class BenchmarkBaseline(string code, string title, double fte, double rate, int capacity, string team, string skill, string employeeId, double threshold, BenchmarkBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public double Rate { get; set; } = rate;
    public int Capacity { get; set; } = capacity;
    public string Team { get; set; } = team;
    public string Skill { get; set; } = skill;
    public string EmployeeId { get; set; } = employeeId;
    public double Threshold { get; set; } = threshold;
    public BenchmarkBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark baseline in status proposed.</summary>
    public static BenchmarkBaseline NewBenchmarkBaseline(string code, string title)
    {
        return new BenchmarkBaseline(code, title, 0.0, 0.0, 0, "", "", "", 0.0, BenchmarkBaselineStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkBaselineStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkBaselineStatus next)
    {
        switch (Status)
        {
            case BenchmarkBaselineStatus.Proposed:
                return next == BenchmarkBaselineStatus.Approved || next == BenchmarkBaselineStatus.Rejected;
            case BenchmarkBaselineStatus.Approved:
                switch (next)
                {
                    case BenchmarkBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
            case BenchmarkBaselineStatus.Rejected:
                return next == BenchmarkBaselineStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(BenchmarkBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"benchmark baseline"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Capacity + Fte, 2);
    }

    public BenchmarkBaseline Copy()
    {
        return new BenchmarkBaseline(Code, Title, Fte, Rate, Capacity, Team, Skill, EmployeeId, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
