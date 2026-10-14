namespace Acme.Workforce.Assets.Refresh.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh estimate is in its life cycle.</summary>
public enum RefreshEstimateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a refresh estimate and its current status.</summary>
public sealed class RefreshEstimate(string code, string name, string managerId, int graceDays, int attempts, int version, int retries, bool approved, string description, string summary, string badge, RefreshEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ManagerId { get; set; } = managerId;
    public int GraceDays { get; set; } = graceDays;
    public int Attempts { get; set; } = attempts;
    public int Version { get; set; } = version;
    public int Retries { get; set; } = retries;
    public bool Approved { get; set; } = approved;
    public string Description { get; set; } = description;
    public string Summary { get; set; } = summary;
    public string Badge { get; set; } = badge;
    public RefreshEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a refresh estimate in status requested.</summary>
    public static RefreshEstimate NewRefreshEstimate(string code, string name)
    {
        return new RefreshEstimate(code, name, "", 0, 0, 0, 0, false, "", "", "", RefreshEstimateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshEstimateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshEstimateStatus next)
    {
        switch (Status)
        {
            case RefreshEstimateStatus.Requested:
                return next == RefreshEstimateStatus.Revoked;
            case RefreshEstimateStatus.Granted:
                return next == RefreshEstimateStatus.Expired;
            case RefreshEstimateStatus.Revoked:
                return next == RefreshEstimateStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RefreshEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"refresh estimate"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Attempts * (double)Retries, 2);
    }

    public RefreshEstimate Copy()
    {
        return new RefreshEstimate(
            Code,
            Name,
            ManagerId,
            GraceDays,
            Attempts,
            Version,
            Retries,
            Approved,
            Description,
            Summary,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
