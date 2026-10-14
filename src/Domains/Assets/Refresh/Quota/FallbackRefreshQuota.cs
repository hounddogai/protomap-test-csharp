namespace Acme.Workforce.Assets.Refresh.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback refresh quota is in its life cycle.</summary>
public enum FallbackRefreshQuotaStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a fallback refresh quota and its current status.</summary>
public sealed class FallbackRefreshQuota(string code, string name, string locale, string managerId, double amount, string source, List<string> tags, int batchSize, string externalRef, double hours, double maximum, FallbackRefreshQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public string ManagerId { get; set; } = managerId;
    public double Amount { get; set; } = amount;
    public string Source { get; set; } = source;
    public List<string> Tags { get; set; } = tags;
    public int BatchSize { get; set; } = batchSize;
    public string ExternalRef { get; set; } = externalRef;
    public double Hours { get; set; } = hours;
    public double Maximum { get; set; } = maximum;
    public FallbackRefreshQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a fallback refresh quota in status draft.</summary>
    public static FallbackRefreshQuota NewFallbackRefreshQuota(string code, string name)
    {
        return new FallbackRefreshQuota(
            code,
            name,
            "",
            "",
            0.0,
            "",
            new List<string>(),
            0,
            "",
            0.0,
            0.0,
            FallbackRefreshQuotaStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackRefreshQuotaStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackRefreshQuotaStatus next)
    {
        switch (Status)
        {
            case FallbackRefreshQuotaStatus.Draft:
                switch (next)
                {
                    case FallbackRefreshQuotaStatus.Pending:
                    case FallbackRefreshQuotaStatus.Active:
                        return true;
                }
                return false;
            case FallbackRefreshQuotaStatus.Pending:
                switch (next)
                {
                    case FallbackRefreshQuotaStatus.Active:
                        return true;
                }
                return false;
            case FallbackRefreshQuotaStatus.Active:
                return next == FallbackRefreshQuotaStatus.Closed;
            case FallbackRefreshQuotaStatus.Suspended:
                switch (next)
                {
                    case FallbackRefreshQuotaStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackRefreshQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback refresh quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double ProjectedValue()
    {
        return Math.Round(Amount + (double)BatchSize, 2);
    }

    public FallbackRefreshQuota Copy()
    {
        return new FallbackRefreshQuota(
            Code,
            Name,
            Locale,
            ManagerId,
            Amount,
            Source,
            Tags,
            BatchSize,
            ExternalRef,
            Hours,
            Maximum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
