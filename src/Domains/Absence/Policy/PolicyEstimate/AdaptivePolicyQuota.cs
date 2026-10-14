namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive policy quota is in its life cycle.</summary>
public enum AdaptivePolicyQuotaStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A adaptive policy quota. Its code is unique within a tenant.</summary>
public sealed class AdaptivePolicyQuota(string code, string title, int startsAt, double hours, string employeeId, bool active, double weight, string currency, string label, AdaptivePolicyQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int StartsAt { get; set; } = startsAt;
    public double Hours { get; set; } = hours;
    public string EmployeeId { get; set; } = employeeId;
    public bool Active { get; set; } = active;
    public double Weight { get; set; } = weight;
    public string Currency { get; set; } = currency;
    public string Label { get; set; } = label;
    public AdaptivePolicyQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive policy quota in status requested.</summary>
    public static AdaptivePolicyQuota NewAdaptivePolicyQuota(string code, string title)
    {
        return new AdaptivePolicyQuota(
            code,
            title,
            0,
            0.0,
            "",
            false,
            0.0,
            "",
            "",
            AdaptivePolicyQuotaStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptivePolicyQuotaStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptivePolicyQuotaStatus next)
    {
        switch (Status)
        {
            case AdaptivePolicyQuotaStatus.Requested:
                return next == AdaptivePolicyQuotaStatus.Granted || next == AdaptivePolicyQuotaStatus.Expired;
            case AdaptivePolicyQuotaStatus.Granted:
                switch (next)
                {
                    case AdaptivePolicyQuotaStatus.Revoked:
                        return true;
                }
                return false;
            case AdaptivePolicyQuotaStatus.Revoked:
                return next == AdaptivePolicyQuotaStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AdaptivePolicyQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive policy quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"adaptive policy quota"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Weight * (double)StartsAt, 2);
    }

    public AdaptivePolicyQuota Copy()
    {
        return new AdaptivePolicyQuota(
            Code,
            Title,
            StartsAt,
            Hours,
            EmployeeId,
            Active,
            Weight,
            Currency,
            Label,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
