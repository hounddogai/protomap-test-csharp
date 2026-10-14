namespace Acme.Workforce.Absence.Balance.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling balance quota is in its life cycle.</summary>
public enum RollingBalanceQuotaStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A rolling balance quota as the balance team keeps it.</summary>
public sealed class RollingBalanceQuota(string code, string title, double floorValue, int limit, string currency, int tenureMonths, bool archived, List<string> tags, string unit, int endsAt, RollingBalanceQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public int Limit { get; set; } = limit;
    public string Currency { get; set; } = currency;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Archived { get; set; } = archived;
    public List<string> Tags { get; set; } = tags;
    public string Unit { get; set; } = unit;
    public int EndsAt { get; set; } = endsAt;
    public RollingBalanceQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a rolling balance quota in status proposed.</summary>
    public static RollingBalanceQuota NewRollingBalanceQuota(string code, string title)
    {
        return new RollingBalanceQuota(
            code,
            title,
            0.0,
            0,
            "",
            0,
            false,
            new List<string>(),
            "",
            0,
            RollingBalanceQuotaStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingBalanceQuotaStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingBalanceQuotaStatus next)
    {
        switch (Status)
        {
            case RollingBalanceQuotaStatus.Proposed:
                switch (next)
                {
                    case RollingBalanceQuotaStatus.Withdrawn:
                        return true;
                }
                return false;
            case RollingBalanceQuotaStatus.Approved:
                switch (next)
                {
                    case RollingBalanceQuotaStatus.Rejected:
                    case RollingBalanceQuotaStatus.Withdrawn:
                        return true;
                }
                return false;
            case RollingBalanceQuotaStatus.Rejected:
                return next == RollingBalanceQuotaStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RollingBalanceQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling balance quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"rolling balance quota"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetLoad()
    {
        return Math.Round((double)Limit * FloorValue, 2);
    }

    public RollingBalanceQuota Copy()
    {
        return new RollingBalanceQuota(
            Code,
            Title,
            FloorValue,
            Limit,
            Currency,
            TenureMonths,
            Archived,
            Tags,
            Unit,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
