namespace Acme.Workforce.Assets.Refresh.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh quota is in its life cycle.</summary>
public enum RefreshQuotaStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a refresh quota and its current status.</summary>
public sealed class RefreshQuota(string code, string label, int tenureMonths, string country, string title, bool active, bool pinned, double ceiling, string notes, double hours, RefreshQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Country { get; set; } = country;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public bool Pinned { get; set; } = pinned;
    public double Ceiling { get; set; } = ceiling;
    public string Notes { get; set; } = notes;
    public double Hours { get; set; } = hours;
    public RefreshQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a refresh quota in status draft.</summary>
    public static RefreshQuota NewRefreshQuota(string code, string label)
    {
        return new RefreshQuota(code, label, 0, "", "", false, false, 0.0, "", 0.0, RefreshQuotaStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshQuotaStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshQuotaStatus next)
    {
        switch (Status)
        {
            case RefreshQuotaStatus.Draft:
                switch (next)
                {
                    case RefreshQuotaStatus.Active:
                    case RefreshQuotaStatus.Closed:
                        return true;
                }
                return false;
            case RefreshQuotaStatus.Pending:
                return next == RefreshQuotaStatus.Active || next == RefreshQuotaStatus.Suspended;
            case RefreshQuotaStatus.Active:
                switch (next)
                {
                    case RefreshQuotaStatus.Suspended:
                    case RefreshQuotaStatus.Closed:
                        return true;
                }
                return false;
            case RefreshQuotaStatus.Suspended:
                return next == RefreshQuotaStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RefreshQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)TenureMonths - Hours, 2);
    }

    public RefreshQuota Copy()
    {
        return new RefreshQuota(
            Code,
            Label,
            TenureMonths,
            Country,
            Title,
            Active,
            Pinned,
            Ceiling,
            Notes,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
