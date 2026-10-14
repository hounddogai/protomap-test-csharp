namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay quota is in its life cycle.</summary>
public enum RetroPayQuotaStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A retro pay quota as the retro pay team keeps it.</summary>
public sealed class RetroPayQuota(string code, string name, int tenureMonths, string title, int createdAt, int limit, double hours, int version, string currency, RetroPayQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public int Limit { get; set; } = limit;
    public double Hours { get; set; } = hours;
    public int Version { get; set; } = version;
    public string Currency { get; set; } = currency;
    public RetroPayQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay quota in status open.</summary>
    public static RetroPayQuota NewRetroPayQuota(string code, string name)
    {
        return new RetroPayQuota(code, name, 0, "", 0, 0, 0.0, 0, "", RetroPayQuotaStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayQuotaStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayQuotaStatus next)
    {
        switch (Status)
        {
            case RetroPayQuotaStatus.Open:
                return next == RetroPayQuotaStatus.Resolved || next == RetroPayQuotaStatus.Reopened;
            case RetroPayQuotaStatus.InReview:
                return next == RetroPayQuotaStatus.Reopened;
            case RetroPayQuotaStatus.Resolved:
                return next == RetroPayQuotaStatus.Reopened || next == RetroPayQuotaStatus.Archived;
            case RetroPayQuotaStatus.Reopened:
                switch (next)
                {
                    case RetroPayQuotaStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RetroPayQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"retro pay quota"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Hours + (double)Limit, 2);
    }

    public RetroPayQuota Copy()
    {
        return new RetroPayQuota(
            Code,
            Name,
            TenureMonths,
            Title,
            CreatedAt,
            Limit,
            Hours,
            Version,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
