namespace Acme.Workforce.Talent.Pool.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool preference is in its life cycle.</summary>
public enum PoolPreferenceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A pool preference. Its code is unique within a tenant.</summary>
public sealed class PoolPreference(string code, string label, int windowDays, int startsAt, int version, double score, int retries, double fte, string country, double margin, int tenureMonths, PoolPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public int StartsAt { get; set; } = startsAt;
    public int Version { get; set; } = version;
    public double Score { get; set; } = score;
    public int Retries { get; set; } = retries;
    public double Fte { get; set; } = fte;
    public string Country { get; set; } = country;
    public double Margin { get; set; } = margin;
    public int TenureMonths { get; set; } = tenureMonths;
    public PoolPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a pool preference in status open.</summary>
    public static PoolPreference NewPoolPreference(string code, string label)
    {
        return new PoolPreference(code, label, 0, 0, 0, 0.0, 0, 0.0, "", 0.0, 0, PoolPreferenceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolPreferenceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolPreferenceStatus next)
    {
        switch (Status)
        {
            case PoolPreferenceStatus.Open:
                return next == PoolPreferenceStatus.Reopened;
            case PoolPreferenceStatus.InReview:
                switch (next)
                {
                    case PoolPreferenceStatus.Archived:
                        return true;
                }
                return false;
            case PoolPreferenceStatus.Resolved:
                switch (next)
                {
                    case PoolPreferenceStatus.Reopened:
                        return true;
                }
                return false;
            case PoolPreferenceStatus.Reopened:
                switch (next)
                {
                    case PoolPreferenceStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedLoad()
    {
        return Math.Round(Margin + (double)Version, 2);
    }

    public PoolPreference Copy()
    {
        return new PoolPreference(
            Code,
            Label,
            WindowDays,
            StartsAt,
            Version,
            Score,
            Retries,
            Fte,
            Country,
            Margin,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
