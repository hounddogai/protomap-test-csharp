namespace Acme.Workforce.Orgdesign.CostModel.ModelPreference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling cost model preference is in its life cycle.</summary>
public enum RollingCostModelPreferenceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One rolling cost model preference, with what the planners need to act on it.</summary>
public sealed class RollingCostModelPreference(string code, string title, int leaveDays, string region, string team, double minimum, bool archived, int expiresAt, string unit, int tenureMonths, RollingCostModelPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public string Region { get; set; } = region;
    public string Team { get; set; } = team;
    public double Minimum { get; set; } = minimum;
    public bool Archived { get; set; } = archived;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Unit { get; set; } = unit;
    public int TenureMonths { get; set; } = tenureMonths;
    public RollingCostModelPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a rolling cost model preference in status draft.</summary>
    public static RollingCostModelPreference NewRollingCostModelPreference(string code, string title)
    {
        return new RollingCostModelPreference(
            code,
            title,
            0,
            "",
            "",
            0.0,
            false,
            0,
            "",
            0,
            RollingCostModelPreferenceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingCostModelPreferenceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingCostModelPreferenceStatus next)
    {
        switch (Status)
        {
            case RollingCostModelPreferenceStatus.Draft:
                return next == RollingCostModelPreferenceStatus.Suspended || next == RollingCostModelPreferenceStatus.Closed;
            case RollingCostModelPreferenceStatus.Pending:
                return next == RollingCostModelPreferenceStatus.Active || next == RollingCostModelPreferenceStatus.Suspended;
            case RollingCostModelPreferenceStatus.Active:
                return next == RollingCostModelPreferenceStatus.Suspended || next == RollingCostModelPreferenceStatus.Closed;
            case RollingCostModelPreferenceStatus.Suspended:
                return next == RollingCostModelPreferenceStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RollingCostModelPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling cost model preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)ExpiresAt - (double)LeaveDays, 2);
    }

    public RollingCostModelPreference Copy()
    {
        return new RollingCostModelPreference(
            Code,
            Title,
            LeaveDays,
            Region,
            Team,
            Minimum,
            Archived,
            ExpiresAt,
            Unit,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
