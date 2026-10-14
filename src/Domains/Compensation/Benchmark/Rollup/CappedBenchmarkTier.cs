namespace Acme.Workforce.Compensation.Benchmark.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped benchmark tier is in its life cycle.</summary>
public enum CappedBenchmarkTierStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One capped benchmark tier, with what the planners need to act on it.</summary>
public sealed class CappedBenchmarkTier(string code, string label, double hours, double rating, int attempts, string grade, CappedBenchmarkTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public double Rating { get; set; } = rating;
    public int Attempts { get; set; } = attempts;
    public string Grade { get; set; } = grade;
    public CappedBenchmarkTierStatus Status { get; set; } = status;

    /// <summary>Starts a capped benchmark tier in status idle.</summary>
    public static CappedBenchmarkTier NewCappedBenchmarkTier(string code, string label)
    {
        return new CappedBenchmarkTier(code, label, 0.0, 0.0, 0, "", CappedBenchmarkTierStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedBenchmarkTierStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedBenchmarkTierStatus next)
    {
        switch (Status)
        {
            case CappedBenchmarkTierStatus.Idle:
                return next == CappedBenchmarkTierStatus.Warming || next == CappedBenchmarkTierStatus.Ready;
            case CappedBenchmarkTierStatus.Warming:
                switch (next)
                {
                    case CappedBenchmarkTierStatus.Ready:
                    case CappedBenchmarkTierStatus.Draining:
                        return true;
                }
                return false;
            case CappedBenchmarkTierStatus.Ready:
                return next == CappedBenchmarkTierStatus.Draining;
            case CappedBenchmarkTierStatus.Draining:
                return next == CappedBenchmarkTierStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(CappedBenchmarkTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped benchmark tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Attempts - Rating, 2);
    }

    public CappedBenchmarkTier Copy()
    {
        return new CappedBenchmarkTier(Code, Label, Hours, Rating, Attempts, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
