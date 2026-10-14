namespace Acme.Workforce.Compensation.Benchmark.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark segment is in its life cycle.</summary>
public enum BenchmarkSegmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a benchmark segment and its current status.</summary>
public sealed class BenchmarkSegment(string code, string label, int retries, List<string> tags, string checksum, int windowDays, double score, BenchmarkSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public List<string> Tags { get; set; } = tags;
    public string Checksum { get; set; } = checksum;
    public int WindowDays { get; set; } = windowDays;
    public double Score { get; set; } = score;
    public BenchmarkSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark segment in status created.</summary>
    public static BenchmarkSegment NewBenchmarkSegment(string code, string label)
    {
        return new BenchmarkSegment(code, label, 0, new List<string>(), "", 0, 0.0, BenchmarkSegmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkSegmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkSegmentStatus next)
    {
        switch (Status)
        {
            case BenchmarkSegmentStatus.Created:
                return next == BenchmarkSegmentStatus.Validated || next == BenchmarkSegmentStatus.Published;
            case BenchmarkSegmentStatus.Validated:
                switch (next)
                {
                    case BenchmarkSegmentStatus.Published:
                    case BenchmarkSegmentStatus.Retired:
                        return true;
                }
                return false;
            case BenchmarkSegmentStatus.Published:
                return next == BenchmarkSegmentStatus.Retired;
        }
        return false;
    }

    public void MoveTo(BenchmarkSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
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

    public double EffectiveVolume()
    {
        return Math.Round(Score + (double)WindowDays, 2);
    }

    public BenchmarkSegment Copy()
    {
        return new BenchmarkSegment(Code, Label, Retries, Tags, Checksum, WindowDays, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
