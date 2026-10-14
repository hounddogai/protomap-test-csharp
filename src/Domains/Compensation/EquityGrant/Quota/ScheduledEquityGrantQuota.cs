namespace Acme.Workforce.Compensation.EquityGrant.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled equity grant quota is in its life cycle.</summary>
public enum ScheduledEquityGrantQuotaStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A scheduled equity grant quota as the equity grant team keeps it.</summary>
public sealed class ScheduledEquityGrantQuota(string code, string title, string category, int quantity, string currency, int version, ScheduledEquityGrantQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public int Quantity { get; set; } = quantity;
    public string Currency { get; set; } = currency;
    public int Version { get; set; } = version;
    public ScheduledEquityGrantQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled equity grant quota in status created.</summary>
    public static ScheduledEquityGrantQuota NewScheduledEquityGrantQuota(string code, string title)
    {
        return new ScheduledEquityGrantQuota(code, title, "", 0, "", 0, ScheduledEquityGrantQuotaStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledEquityGrantQuotaStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledEquityGrantQuotaStatus next)
    {
        switch (Status)
        {
            case ScheduledEquityGrantQuotaStatus.Created:
                return next == ScheduledEquityGrantQuotaStatus.Validated;
            case ScheduledEquityGrantQuotaStatus.Validated:
                return next == ScheduledEquityGrantQuotaStatus.Published || next == ScheduledEquityGrantQuotaStatus.Retired;
            case ScheduledEquityGrantQuotaStatus.Published:
                return next == ScheduledEquityGrantQuotaStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ScheduledEquityGrantQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled equity grant quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Quantity * (double)Version, 2);
    }

    public ScheduledEquityGrantQuota Copy()
    {
        return new ScheduledEquityGrantQuota(Code, Title, Category, Quantity, Currency, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
