namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending refresh quota is in its life cycle.</summary>
public enum PendingRefreshQuotaStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One pending refresh quota, with what the planners need to act on it.</summary>
public sealed class PendingRefreshQuota(string code, string title, double minimum, string summary, int priority, double budget, int startsAt, string timezone, int batchSize, PendingRefreshQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public string Summary { get; set; } = summary;
    public int Priority { get; set; } = priority;
    public double Budget { get; set; } = budget;
    public int StartsAt { get; set; } = startsAt;
    public string Timezone { get; set; } = timezone;
    public int BatchSize { get; set; } = batchSize;
    public PendingRefreshQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a pending refresh quota in status requested.</summary>
    public static PendingRefreshQuota NewPendingRefreshQuota(string code, string title)
    {
        return new PendingRefreshQuota(code, title, 0.0, "", 0, 0.0, 0, "", 0, PendingRefreshQuotaStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingRefreshQuotaStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingRefreshQuotaStatus next)
    {
        switch (Status)
        {
            case PendingRefreshQuotaStatus.Requested:
                switch (next)
                {
                    case PendingRefreshQuotaStatus.Revoked:
                        return true;
                }
                return false;
            case PendingRefreshQuotaStatus.Granted:
                return next == PendingRefreshQuotaStatus.Revoked || next == PendingRefreshQuotaStatus.Expired;
            case PendingRefreshQuotaStatus.Revoked:
                switch (next)
                {
                    case PendingRefreshQuotaStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingRefreshQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending refresh quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"pending refresh quota"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)BatchSize - Budget, 2);
    }

    public PendingRefreshQuota Copy()
    {
        return new PendingRefreshQuota(
            Code,
            Title,
            Minimum,
            Summary,
            Priority,
            Budget,
            StartsAt,
            Timezone,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
