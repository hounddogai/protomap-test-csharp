namespace Acme.Workforce.Compensation.Benchmark.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark request is in its life cycle.</summary>
public enum BenchmarkRequestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A benchmark request. Its code is unique within a tenant.</summary>
public sealed class BenchmarkRequest(string code, string name, string skill, int sequence, string notes, string source, BenchmarkRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Skill { get; set; } = skill;
    public int Sequence { get; set; } = sequence;
    public string Notes { get; set; } = notes;
    public string Source { get; set; } = source;
    public BenchmarkRequestStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark request in status created.</summary>
    public static BenchmarkRequest NewBenchmarkRequest(string code, string name)
    {
        return new BenchmarkRequest(code, name, "", 0, "", "", BenchmarkRequestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkRequestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkRequestStatus next)
    {
        switch (Status)
        {
            case BenchmarkRequestStatus.Created:
                switch (next)
                {
                    case BenchmarkRequestStatus.Validated:
                    case BenchmarkRequestStatus.Retired:
                        return true;
                }
                return false;
            case BenchmarkRequestStatus.Validated:
                return next == BenchmarkRequestStatus.Published || next == BenchmarkRequestStatus.Retired;
            case BenchmarkRequestStatus.Published:
                switch (next)
                {
                    case BenchmarkRequestStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BenchmarkRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Sequence;
    }

    public BenchmarkRequest Copy()
    {
        return new BenchmarkRequest(Code, Name, Skill, Sequence, Notes, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
