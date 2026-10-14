namespace Acme.Workforce.Absence.Accrual.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual quota is in its life cycle.</summary>
public enum AccrualQuotaStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a accrual quota and its current status.</summary>
public sealed class AccrualQuota(string code, string label, int priority, int version, string notes, string costCenter, string grade, string currency, string contactEmail, string ownerId, string timezone, AccrualQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public int Version { get; set; } = version;
    public string Notes { get; set; } = notes;
    public string CostCenter { get; set; } = costCenter;
    public string Grade { get; set; } = grade;
    public string Currency { get; set; } = currency;
    public string ContactEmail { get; set; } = contactEmail;
    public string OwnerId { get; set; } = ownerId;
    public string Timezone { get; set; } = timezone;
    public AccrualQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a accrual quota in status idle.</summary>
    public static AccrualQuota NewAccrualQuota(string code, string label)
    {
        return new AccrualQuota(code, label, 0, 0, "", "", "", "", "", "", "", AccrualQuotaStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualQuotaStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualQuotaStatus next)
    {
        switch (Status)
        {
            case AccrualQuotaStatus.Idle:
                return next == AccrualQuotaStatus.Draining;
            case AccrualQuotaStatus.Warming:
                switch (next)
                {
                    case AccrualQuotaStatus.Ready:
                        return true;
                }
                return false;
            case AccrualQuotaStatus.Ready:
                return next == AccrualQuotaStatus.Draining || next == AccrualQuotaStatus.Stopped;
            case AccrualQuotaStatus.Draining:
                switch (next)
                {
                    case AccrualQuotaStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccrualQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"accrual quota"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Version + (double)Priority, 2);
    }

    public AccrualQuota Copy()
    {
        return new AccrualQuota(
            Code,
            Label,
            Priority,
            Version,
            Notes,
            CostCenter,
            Grade,
            Currency,
            ContactEmail,
            OwnerId,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
