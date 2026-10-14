namespace Acme.Workforce.Compensation.Benchmark.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark score is in its life cycle.</summary>
public enum BenchmarkScoreStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A benchmark score. Its code is unique within a tenant.</summary>
public sealed class BenchmarkScore(string code, string label, int revision, string grade, string ownerId, bool visible, bool pinned, string source, BenchmarkScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public string Grade { get; set; } = grade;
    public string OwnerId { get; set; } = ownerId;
    public bool Visible { get; set; } = visible;
    public bool Pinned { get; set; } = pinned;
    public string Source { get; set; } = source;
    public BenchmarkScoreStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark score in status new.</summary>
    public static BenchmarkScore NewBenchmarkScore(string code, string label)
    {
        return new BenchmarkScore(code, label, 0, "", "", false, false, "", BenchmarkScoreStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkScoreStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkScoreStatus next)
    {
        switch (Status)
        {
            case BenchmarkScoreStatus.New:
                switch (next)
                {
                    case BenchmarkScoreStatus.Queued:
                    case BenchmarkScoreStatus.Failed:
                        return true;
                }
                return false;
            case BenchmarkScoreStatus.Queued:
                switch (next)
                {
                    case BenchmarkScoreStatus.Failed:
                        return true;
                }
                return false;
            case BenchmarkScoreStatus.Running:
                return next == BenchmarkScoreStatus.Done;
            case BenchmarkScoreStatus.Done:
                switch (next)
                {
                    case BenchmarkScoreStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BenchmarkScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)Revision;
    }

    public BenchmarkScore Copy()
    {
        return new BenchmarkScore(Code, Label, Revision, Grade, OwnerId, Visible, Pinned, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
