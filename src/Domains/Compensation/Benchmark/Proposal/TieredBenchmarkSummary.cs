namespace Acme.Workforce.Compensation.Benchmark.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered benchmark summary is in its life cycle.</summary>
public enum TieredBenchmarkSummaryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A tiered benchmark summary as the benchmark team keeps it.</summary>
public sealed class TieredBenchmarkSummary(string code, string name, int attempts, string phone, string employeeId, string skill, string title, TieredBenchmarkSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public string Phone { get; set; } = phone;
    public string EmployeeId { get; set; } = employeeId;
    public string Skill { get; set; } = skill;
    public string Title { get; set; } = title;
    public TieredBenchmarkSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a tiered benchmark summary in status requested.</summary>
    public static TieredBenchmarkSummary NewTieredBenchmarkSummary(string code, string name)
    {
        return new TieredBenchmarkSummary(code, name, 0, "", "", "", "", TieredBenchmarkSummaryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredBenchmarkSummaryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredBenchmarkSummaryStatus next)
    {
        switch (Status)
        {
            case TieredBenchmarkSummaryStatus.Requested:
                switch (next)
                {
                    case TieredBenchmarkSummaryStatus.Granted:
                    case TieredBenchmarkSummaryStatus.Expired:
                        return true;
                }
                return false;
            case TieredBenchmarkSummaryStatus.Granted:
                switch (next)
                {
                    case TieredBenchmarkSummaryStatus.Revoked:
                    case TieredBenchmarkSummaryStatus.Expired:
                        return true;
                }
                return false;
            case TieredBenchmarkSummaryStatus.Revoked:
                switch (next)
                {
                    case TieredBenchmarkSummaryStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredBenchmarkSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered benchmark summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Attempts;
    }

    public TieredBenchmarkSummary Copy()
    {
        return new TieredBenchmarkSummary(Code, Name, Attempts, Phone, EmployeeId, Skill, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
