namespace Acme.Workforce.Talent.Pool.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal pool tier is in its life cycle.</summary>
public enum SeasonalPoolTierStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One seasonal pool tier, with what the planners need to act on it.</summary>
public sealed class SeasonalPoolTier(string code, string name, string reviewer, int updatedAt, string category, SeasonalPoolTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Reviewer { get; set; } = reviewer;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Category { get; set; } = category;
    public SeasonalPoolTierStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal pool tier in status requested.</summary>
    public static SeasonalPoolTier NewSeasonalPoolTier(string code, string name)
    {
        return new SeasonalPoolTier(code, name, "", 0, "", SeasonalPoolTierStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalPoolTierStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalPoolTierStatus next)
    {
        switch (Status)
        {
            case SeasonalPoolTierStatus.Requested:
                switch (next)
                {
                    case SeasonalPoolTierStatus.Granted:
                    case SeasonalPoolTierStatus.Expired:
                        return true;
                }
                return false;
            case SeasonalPoolTierStatus.Granted:
                switch (next)
                {
                    case SeasonalPoolTierStatus.Revoked:
                        return true;
                }
                return false;
            case SeasonalPoolTierStatus.Revoked:
                switch (next)
                {
                    case SeasonalPoolTierStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalPoolTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal pool tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)UpdatedAt;
    }

    public SeasonalPoolTier Copy()
    {
        return new SeasonalPoolTier(Code, Name, Reviewer, UpdatedAt, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
