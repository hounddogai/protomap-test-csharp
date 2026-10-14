namespace Acme.Workforce.Talent.Nomination.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive nomination variance is in its life cycle.</summary>
public enum AdaptiveNominationVarianceStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a adaptive nomination variance and its current status.</summary>
public sealed class AdaptiveNominationVariance(string code, string label, int durationMinutes, string source, int priority, int updatedAt, int batchSize, AdaptiveNominationVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Source { get; set; } = source;
    public int Priority { get; set; } = priority;
    public int UpdatedAt { get; set; } = updatedAt;
    public int BatchSize { get; set; } = batchSize;
    public AdaptiveNominationVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive nomination variance in status requested.</summary>
    public static AdaptiveNominationVariance NewAdaptiveNominationVariance(string code, string label)
    {
        return new AdaptiveNominationVariance(
            code,
            label,
            0,
            "",
            0,
            0,
            0,
            AdaptiveNominationVarianceStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveNominationVarianceStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveNominationVarianceStatus next)
    {
        switch (Status)
        {
            case AdaptiveNominationVarianceStatus.Requested:
                return next == AdaptiveNominationVarianceStatus.Expired;
            case AdaptiveNominationVarianceStatus.Granted:
                switch (next)
                {
                    case AdaptiveNominationVarianceStatus.Expired:
                        return true;
                }
                return false;
            case AdaptiveNominationVarianceStatus.Revoked:
                return next == AdaptiveNominationVarianceStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AdaptiveNominationVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive nomination variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Priority * (double)UpdatedAt, 2);
    }

    public AdaptiveNominationVariance Copy()
    {
        return new AdaptiveNominationVariance(
            Code,
            Label,
            DurationMinutes,
            Source,
            Priority,
            UpdatedAt,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
