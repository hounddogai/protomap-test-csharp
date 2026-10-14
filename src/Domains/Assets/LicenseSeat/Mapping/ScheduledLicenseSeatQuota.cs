namespace Acme.Workforce.Assets.LicenseSeat.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled license seat quota is in its life cycle.</summary>
public enum ScheduledLicenseSeatQuotaStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A scheduled license seat quota as the license seat team keeps it.</summary>
public sealed class ScheduledLicenseSeatQuota(string code, string title, int attempts, int updatedAt, double maximum, ScheduledLicenseSeatQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Maximum { get; set; } = maximum;
    public ScheduledLicenseSeatQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled license seat quota in status open.</summary>
    public static ScheduledLicenseSeatQuota NewScheduledLicenseSeatQuota(string code, string title)
    {
        return new ScheduledLicenseSeatQuota(code, title, 0, 0, 0.0, ScheduledLicenseSeatQuotaStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledLicenseSeatQuotaStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledLicenseSeatQuotaStatus next)
    {
        switch (Status)
        {
            case ScheduledLicenseSeatQuotaStatus.Open:
                return next == ScheduledLicenseSeatQuotaStatus.Resolved;
            case ScheduledLicenseSeatQuotaStatus.InReview:
                return next == ScheduledLicenseSeatQuotaStatus.Reopened;
            case ScheduledLicenseSeatQuotaStatus.Resolved:
                return next == ScheduledLicenseSeatQuotaStatus.Archived;
            case ScheduledLicenseSeatQuotaStatus.Reopened:
                return next == ScheduledLicenseSeatQuotaStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ScheduledLicenseSeatQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled license seat quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Attempts + (double)UpdatedAt, 2);
    }

    public ScheduledLicenseSeatQuota Copy()
    {
        return new ScheduledLicenseSeatQuota(Code, Title, Attempts, UpdatedAt, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
