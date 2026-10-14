namespace Acme.Workforce.Compensation.BonusPool.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback bonus pool profile is in its life cycle.</summary>
public enum FallbackBonusPoolProfileStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One fallback bonus pool profile, with what the planners need to act on it.</summary>
public sealed class FallbackBonusPoolProfile(string code, string title, double rating, double threshold, int batchSize, FallbackBonusPoolProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rating { get; set; } = rating;
    public double Threshold { get; set; } = threshold;
    public int BatchSize { get; set; } = batchSize;
    public FallbackBonusPoolProfileStatus Status { get; set; } = status;

    /// <summary>Starts a fallback bonus pool profile in status idle.</summary>
    public static FallbackBonusPoolProfile NewFallbackBonusPoolProfile(string code, string title)
    {
        return new FallbackBonusPoolProfile(code, title, 0.0, 0.0, 0, FallbackBonusPoolProfileStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackBonusPoolProfileStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackBonusPoolProfileStatus next)
    {
        switch (Status)
        {
            case FallbackBonusPoolProfileStatus.Idle:
                return next == FallbackBonusPoolProfileStatus.Warming || next == FallbackBonusPoolProfileStatus.Stopped;
            case FallbackBonusPoolProfileStatus.Warming:
                return next == FallbackBonusPoolProfileStatus.Draining || next == FallbackBonusPoolProfileStatus.Stopped;
            case FallbackBonusPoolProfileStatus.Ready:
                switch (next)
                {
                    case FallbackBonusPoolProfileStatus.Draining:
                    case FallbackBonusPoolProfileStatus.Stopped:
                        return true;
                }
                return false;
            case FallbackBonusPoolProfileStatus.Draining:
                return next == FallbackBonusPoolProfileStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(FallbackBonusPoolProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback bonus pool profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)BatchSize * Rating, 2);
    }

    public FallbackBonusPoolProfile Copy()
    {
        return new FallbackBonusPoolProfile(Code, Title, Rating, Threshold, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
