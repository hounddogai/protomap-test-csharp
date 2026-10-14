namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg preference is in its life cycle.</summary>
public enum ReorgPreferenceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a reorg preference and its current status.</summary>
public sealed class ReorgPreference(string code, string title, int durationMinutes, string source, int batchSize, int tenureMonths, double weight, string team, int retries, bool approved, int expiresAt, ReorgPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Source { get; set; } = source;
    public int BatchSize { get; set; } = batchSize;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Weight { get; set; } = weight;
    public string Team { get; set; } = team;
    public int Retries { get; set; } = retries;
    public bool Approved { get; set; } = approved;
    public int ExpiresAt { get; set; } = expiresAt;
    public ReorgPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a reorg preference in status open.</summary>
    public static ReorgPreference NewReorgPreference(string code, string title)
    {
        return new ReorgPreference(code, title, 0, "", 0, 0, 0.0, "", 0, false, 0, ReorgPreferenceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgPreferenceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgPreferenceStatus next)
    {
        switch (Status)
        {
            case ReorgPreferenceStatus.Open:
                switch (next)
                {
                    case ReorgPreferenceStatus.InReview:
                        return true;
                }
                return false;
            case ReorgPreferenceStatus.InReview:
                return next == ReorgPreferenceStatus.Resolved || next == ReorgPreferenceStatus.Reopened;
            case ReorgPreferenceStatus.Resolved:
                return next == ReorgPreferenceStatus.Reopened || next == ReorgPreferenceStatus.Archived;
            case ReorgPreferenceStatus.Reopened:
                switch (next)
                {
                    case ReorgPreferenceStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReorgPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Retries - (double)DurationMinutes, 2);
    }

    public ReorgPreference Copy()
    {
        return new ReorgPreference(
            Code,
            Title,
            DurationMinutes,
            Source,
            BatchSize,
            TenureMonths,
            Weight,
            Team,
            Retries,
            Approved,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
