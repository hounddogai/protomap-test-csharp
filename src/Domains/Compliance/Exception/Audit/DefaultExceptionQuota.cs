namespace Acme.Workforce.Compliance.Exception.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default exception quota is in its life cycle.</summary>
public enum DefaultExceptionQuotaStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A default exception quota as the exception team keeps it.</summary>
public sealed class DefaultExceptionQuota(string code, string title, int tenureMonths, double cost, string ownerId, double rating, double margin, int updatedAt, string country, DefaultExceptionQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Cost { get; set; } = cost;
    public string OwnerId { get; set; } = ownerId;
    public double Rating { get; set; } = rating;
    public double Margin { get; set; } = margin;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Country { get; set; } = country;
    public DefaultExceptionQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a default exception quota in status planned.</summary>
    public static DefaultExceptionQuota NewDefaultExceptionQuota(string code, string title)
    {
        return new DefaultExceptionQuota(
            code,
            title,
            0,
            0.0,
            "",
            0.0,
            0.0,
            0,
            "",
            DefaultExceptionQuotaStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultExceptionQuotaStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultExceptionQuotaStatus next)
    {
        switch (Status)
        {
            case DefaultExceptionQuotaStatus.Planned:
                switch (next)
                {
                    case DefaultExceptionQuotaStatus.Scheduled:
                    case DefaultExceptionQuotaStatus.Cancelled:
                        return true;
                }
                return false;
            case DefaultExceptionQuotaStatus.Scheduled:
                return next == DefaultExceptionQuotaStatus.Cancelled;
            case DefaultExceptionQuotaStatus.InProgress:
                switch (next)
                {
                    case DefaultExceptionQuotaStatus.Completed:
                        return true;
                }
                return false;
            case DefaultExceptionQuotaStatus.Completed:
                switch (next)
                {
                    case DefaultExceptionQuotaStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultExceptionQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default exception quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"default exception quota"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Rating + (double)TenureMonths, 2);
    }

    public DefaultExceptionQuota Copy()
    {
        return new DefaultExceptionQuota(
            Code,
            Title,
            TenureMonths,
            Cost,
            OwnerId,
            Rating,
            Margin,
            UpdatedAt,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
