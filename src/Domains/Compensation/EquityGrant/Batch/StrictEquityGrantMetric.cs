namespace Acme.Workforce.Compensation.EquityGrant.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict equity grant metric is in its life cycle.</summary>
public enum StrictEquityGrantMetricStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One strict equity grant metric, with what the planners need to act on it.</summary>
public sealed class StrictEquityGrantMetric(string code, string title, int limit, string unit, double maximum, double rating, string checksum, string team, int createdAt, int expiresAt, StrictEquityGrantMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public string Unit { get; set; } = unit;
    public double Maximum { get; set; } = maximum;
    public double Rating { get; set; } = rating;
    public string Checksum { get; set; } = checksum;
    public string Team { get; set; } = team;
    public int CreatedAt { get; set; } = createdAt;
    public int ExpiresAt { get; set; } = expiresAt;
    public StrictEquityGrantMetricStatus Status { get; set; } = status;

    /// <summary>Starts a strict equity grant metric in status requested.</summary>
    public static StrictEquityGrantMetric NewStrictEquityGrantMetric(string code, string title)
    {
        return new StrictEquityGrantMetric(
            code,
            title,
            0,
            "",
            0.0,
            0.0,
            "",
            "",
            0,
            0,
            StrictEquityGrantMetricStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictEquityGrantMetricStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictEquityGrantMetricStatus next)
    {
        switch (Status)
        {
            case StrictEquityGrantMetricStatus.Requested:
                return next == StrictEquityGrantMetricStatus.Granted || next == StrictEquityGrantMetricStatus.Expired;
            case StrictEquityGrantMetricStatus.Granted:
                return next == StrictEquityGrantMetricStatus.Revoked || next == StrictEquityGrantMetricStatus.Expired;
            case StrictEquityGrantMetricStatus.Revoked:
                return next == StrictEquityGrantMetricStatus.Expired;
        }
        return false;
    }

    public void MoveTo(StrictEquityGrantMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict equity grant metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Maximum * (double)ExpiresAt, 2);
    }

    public StrictEquityGrantMetric Copy()
    {
        return new StrictEquityGrantMetric(
            Code,
            Title,
            Limit,
            Unit,
            Maximum,
            Rating,
            Checksum,
            Team,
            CreatedAt,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
