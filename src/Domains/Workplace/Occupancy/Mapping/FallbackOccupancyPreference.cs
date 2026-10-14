namespace Acme.Workforce.Workplace.Occupancy.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback occupancy preference is in its life cycle.</summary>
public enum FallbackOccupancyPreferenceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A fallback occupancy preference as the occupancy team keeps it.</summary>
public sealed class FallbackOccupancyPreference(string code, string label, int expiresAt, int tenureMonths, bool approved, int retries, FallbackOccupancyPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Approved { get; set; } = approved;
    public int Retries { get; set; } = retries;
    public FallbackOccupancyPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a fallback occupancy preference in status draft.</summary>
    public static FallbackOccupancyPreference NewFallbackOccupancyPreference(string code, string label)
    {
        return new FallbackOccupancyPreference(code, label, 0, 0, false, 0, FallbackOccupancyPreferenceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackOccupancyPreferenceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackOccupancyPreferenceStatus next)
    {
        switch (Status)
        {
            case FallbackOccupancyPreferenceStatus.Draft:
                switch (next)
                {
                    case FallbackOccupancyPreferenceStatus.Pending:
                    case FallbackOccupancyPreferenceStatus.Closed:
                        return true;
                }
                return false;
            case FallbackOccupancyPreferenceStatus.Pending:
                switch (next)
                {
                    case FallbackOccupancyPreferenceStatus.Active:
                        return true;
                }
                return false;
            case FallbackOccupancyPreferenceStatus.Active:
                switch (next)
                {
                    case FallbackOccupancyPreferenceStatus.Suspended:
                    case FallbackOccupancyPreferenceStatus.Closed:
                        return true;
                }
                return false;
            case FallbackOccupancyPreferenceStatus.Suspended:
                return next == FallbackOccupancyPreferenceStatus.Closed;
        }
        return false;
    }

    public void MoveTo(FallbackOccupancyPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback occupancy preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Retries + (double)ExpiresAt, 2);
    }

    public FallbackOccupancyPreference Copy()
    {
        return new FallbackOccupancyPreference(Code, Label, ExpiresAt, TenureMonths, Approved, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
