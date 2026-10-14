namespace Acme.Workforce.Talent.Pool.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy pool variance is in its life cycle.</summary>
public enum LegacyPoolVarianceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One legacy pool variance, with what the planners need to act on it.</summary>
public sealed class LegacyPoolVariance(string code, string title, string timezone, double cost, double minimum, int createdAt, double score, double rate, LegacyPoolVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public double Cost { get; set; } = cost;
    public double Minimum { get; set; } = minimum;
    public int CreatedAt { get; set; } = createdAt;
    public double Score { get; set; } = score;
    public double Rate { get; set; } = rate;
    public LegacyPoolVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a legacy pool variance in status open.</summary>
    public static LegacyPoolVariance NewLegacyPoolVariance(string code, string title)
    {
        return new LegacyPoolVariance(code, title, "", 0.0, 0.0, 0, 0.0, 0.0, LegacyPoolVarianceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPoolVarianceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPoolVarianceStatus next)
    {
        switch (Status)
        {
            case LegacyPoolVarianceStatus.Open:
                return next == LegacyPoolVarianceStatus.Resolved || next == LegacyPoolVarianceStatus.Archived;
            case LegacyPoolVarianceStatus.InReview:
                switch (next)
                {
                    case LegacyPoolVarianceStatus.Resolved:
                        return true;
                }
                return false;
            case LegacyPoolVarianceStatus.Resolved:
                switch (next)
                {
                    case LegacyPoolVarianceStatus.Archived:
                        return true;
                }
                return false;
            case LegacyPoolVarianceStatus.Reopened:
                switch (next)
                {
                    case LegacyPoolVarianceStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyPoolVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy pool variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)CreatedAt - Rate, 2);
    }

    public LegacyPoolVariance Copy()
    {
        return new LegacyPoolVariance(Code, Title, Timezone, Cost, Minimum, CreatedAt, Score, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
