namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary accrual tier is in its life cycle.</summary>
public enum PrimaryAccrualTierStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A primary accrual tier as the accrual team keeps it.</summary>
public sealed class PrimaryAccrualTier(string code, string name, string checksum, string category, int endsAt, double budget, PrimaryAccrualTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public string Category { get; set; } = category;
    public int EndsAt { get; set; } = endsAt;
    public double Budget { get; set; } = budget;
    public PrimaryAccrualTierStatus Status { get; set; } = status;

    /// <summary>Starts a primary accrual tier in status requested.</summary>
    public static PrimaryAccrualTier NewPrimaryAccrualTier(string code, string name)
    {
        return new PrimaryAccrualTier(code, name, "", "", 0, 0.0, PrimaryAccrualTierStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryAccrualTierStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryAccrualTierStatus next)
    {
        switch (Status)
        {
            case PrimaryAccrualTierStatus.Requested:
                switch (next)
                {
                    case PrimaryAccrualTierStatus.Revoked:
                    case PrimaryAccrualTierStatus.Expired:
                        return true;
                }
                return false;
            case PrimaryAccrualTierStatus.Granted:
                return next == PrimaryAccrualTierStatus.Revoked;
            case PrimaryAccrualTierStatus.Revoked:
                switch (next)
                {
                    case PrimaryAccrualTierStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryAccrualTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary accrual tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Budget - (double)EndsAt, 2);
    }

    public PrimaryAccrualTier Copy()
    {
        return new PrimaryAccrualTier(Code, Name, Checksum, Category, EndsAt, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
