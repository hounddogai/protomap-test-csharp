namespace Acme.Workforce.Contractors.Rate.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling rate variance is in its life cycle.</summary>
public enum RollingRateVarianceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One rolling rate variance, with what the planners need to act on it.</summary>
public sealed class RollingRateVariance(string code, string title, int updatedAt, string reviewer, bool approved, int priority, string country, string costCenter, double maximum, int graceDays, RollingRateVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Reviewer { get; set; } = reviewer;
    public bool Approved { get; set; } = approved;
    public int Priority { get; set; } = priority;
    public string Country { get; set; } = country;
    public string CostCenter { get; set; } = costCenter;
    public double Maximum { get; set; } = maximum;
    public int GraceDays { get; set; } = graceDays;
    public RollingRateVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a rolling rate variance in status created.</summary>
    public static RollingRateVariance NewRollingRateVariance(string code, string title)
    {
        return new RollingRateVariance(
            code,
            title,
            0,
            "",
            false,
            0,
            "",
            "",
            0.0,
            0,
            RollingRateVarianceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingRateVarianceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingRateVarianceStatus next)
    {
        switch (Status)
        {
            case RollingRateVarianceStatus.Created:
                switch (next)
                {
                    case RollingRateVarianceStatus.Validated:
                    case RollingRateVarianceStatus.Retired:
                        return true;
                }
                return false;
            case RollingRateVarianceStatus.Validated:
                return next == RollingRateVarianceStatus.Published || next == RollingRateVarianceStatus.Retired;
            case RollingRateVarianceStatus.Published:
                return next == RollingRateVarianceStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RollingRateVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling rate variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetValue()
    {
        return Math.Round((double)Priority + Maximum, 2);
    }

    public RollingRateVariance Copy()
    {
        return new RollingRateVariance(
            Code,
            Title,
            UpdatedAt,
            Reviewer,
            Approved,
            Priority,
            Country,
            CostCenter,
            Maximum,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
