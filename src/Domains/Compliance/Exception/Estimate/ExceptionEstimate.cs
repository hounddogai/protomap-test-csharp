namespace Acme.Workforce.Compliance.Exception.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception estimate is in its life cycle.</summary>
public enum ExceptionEstimateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One exception estimate, with what the planners need to act on it.</summary>
public sealed class ExceptionEstimate(string code, string name, double ratio, string timezone, double score, string jobCode, ExceptionEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ratio { get; set; } = ratio;
    public string Timezone { get; set; } = timezone;
    public double Score { get; set; } = score;
    public string JobCode { get; set; } = jobCode;
    public ExceptionEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a exception estimate in status idle.</summary>
    public static ExceptionEstimate NewExceptionEstimate(string code, string name)
    {
        return new ExceptionEstimate(code, name, 0.0, "", 0.0, "", ExceptionEstimateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionEstimateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionEstimateStatus next)
    {
        switch (Status)
        {
            case ExceptionEstimateStatus.Idle:
                return next == ExceptionEstimateStatus.Warming || next == ExceptionEstimateStatus.Ready;
            case ExceptionEstimateStatus.Warming:
                return next == ExceptionEstimateStatus.Draining;
            case ExceptionEstimateStatus.Ready:
                return next == ExceptionEstimateStatus.Draining || next == ExceptionEstimateStatus.Stopped;
            case ExceptionEstimateStatus.Draining:
                switch (next)
                {
                    case ExceptionEstimateStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ExceptionEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Score - Ratio, 2);
    }

    public ExceptionEstimate Copy()
    {
        return new ExceptionEstimate(Code, Name, Ratio, Timezone, Score, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
