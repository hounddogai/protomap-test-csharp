namespace Acme.Workforce.Compliance.TrainingDue.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback training due rollup is in its life cycle.</summary>
public enum FallbackTrainingDueRollupStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a fallback training due rollup and its current status.</summary>
public sealed class FallbackTrainingDueRollup(string code, string title, int graceDays, string source, string region, FallbackTrainingDueRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int GraceDays { get; set; } = graceDays;
    public string Source { get; set; } = source;
    public string Region { get; set; } = region;
    public FallbackTrainingDueRollupStatus Status { get; set; } = status;

    /// <summary>Starts a fallback training due rollup in status requested.</summary>
    public static FallbackTrainingDueRollup NewFallbackTrainingDueRollup(string code, string title)
    {
        return new FallbackTrainingDueRollup(code, title, 0, "", "", FallbackTrainingDueRollupStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackTrainingDueRollupStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackTrainingDueRollupStatus next)
    {
        switch (Status)
        {
            case FallbackTrainingDueRollupStatus.Requested:
                return next == FallbackTrainingDueRollupStatus.Granted || next == FallbackTrainingDueRollupStatus.Revoked;
            case FallbackTrainingDueRollupStatus.Granted:
                return next == FallbackTrainingDueRollupStatus.Revoked;
            case FallbackTrainingDueRollupStatus.Revoked:
                switch (next)
                {
                    case FallbackTrainingDueRollupStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackTrainingDueRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback training due rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)GraceDays;
    }

    public FallbackTrainingDueRollup Copy()
    {
        return new FallbackTrainingDueRollup(Code, Title, GraceDays, Source, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
