namespace Acme.Workforce.Compensation.PayBand.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal pay band quota is in its life cycle.</summary>
public enum SeasonalPayBandQuotaStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A seasonal pay band quota as the pay band team keeps it.</summary>
public sealed class SeasonalPayBandQuota(string code, string label, string jobCode, string employeeId, int graceDays, int sequence, int leaveDays, string region, SeasonalPayBandQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public string EmployeeId { get; set; } = employeeId;
    public int GraceDays { get; set; } = graceDays;
    public int Sequence { get; set; } = sequence;
    public int LeaveDays { get; set; } = leaveDays;
    public string Region { get; set; } = region;
    public SeasonalPayBandQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal pay band quota in status idle.</summary>
    public static SeasonalPayBandQuota NewSeasonalPayBandQuota(string code, string label)
    {
        return new SeasonalPayBandQuota(code, label, "", "", 0, 0, 0, "", SeasonalPayBandQuotaStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalPayBandQuotaStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalPayBandQuotaStatus next)
    {
        switch (Status)
        {
            case SeasonalPayBandQuotaStatus.Idle:
                switch (next)
                {
                    case SeasonalPayBandQuotaStatus.Warming:
                    case SeasonalPayBandQuotaStatus.Ready:
                        return true;
                }
                return false;
            case SeasonalPayBandQuotaStatus.Warming:
                return next == SeasonalPayBandQuotaStatus.Draining || next == SeasonalPayBandQuotaStatus.Stopped;
            case SeasonalPayBandQuotaStatus.Ready:
                return next == SeasonalPayBandQuotaStatus.Draining;
            case SeasonalPayBandQuotaStatus.Draining:
                return next == SeasonalPayBandQuotaStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SeasonalPayBandQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal pay band quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)LeaveDays + (double)Sequence, 2);
    }

    public SeasonalPayBandQuota Copy()
    {
        return new SeasonalPayBandQuota(
            Code,
            Label,
            JobCode,
            EmployeeId,
            GraceDays,
            Sequence,
            LeaveDays,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
