namespace Acme.Workforce.Absence.ReturnPlan.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan quota is in its life cycle.</summary>
public enum ReturnPlanQuotaStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One return plan quota, with what the planners need to act on it.</summary>
public sealed class ReturnPlanQuota(string code, string name, int capacity, int startsAt, int endsAt, int revision, int tenureMonths, double minimum, string costCenter, double floorValue, ReturnPlanQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public int StartsAt { get; set; } = startsAt;
    public int EndsAt { get; set; } = endsAt;
    public int Revision { get; set; } = revision;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Minimum { get; set; } = minimum;
    public string CostCenter { get; set; } = costCenter;
    public double FloorValue { get; set; } = floorValue;
    public ReturnPlanQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a return plan quota in status requested.</summary>
    public static ReturnPlanQuota NewReturnPlanQuota(string code, string name)
    {
        return new ReturnPlanQuota(code, name, 0, 0, 0, 0, 0, 0.0, "", 0.0, ReturnPlanQuotaStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanQuotaStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanQuotaStatus next)
    {
        switch (Status)
        {
            case ReturnPlanQuotaStatus.Requested:
                return next == ReturnPlanQuotaStatus.Revoked || next == ReturnPlanQuotaStatus.Expired;
            case ReturnPlanQuotaStatus.Granted:
                return next == ReturnPlanQuotaStatus.Revoked || next == ReturnPlanQuotaStatus.Expired;
            case ReturnPlanQuotaStatus.Revoked:
                return next == ReturnPlanQuotaStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ReturnPlanQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"return plan quota"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Revision - FloorValue, 2);
    }

    public ReturnPlanQuota Copy()
    {
        return new ReturnPlanQuota(
            Code,
            Name,
            Capacity,
            StartsAt,
            EndsAt,
            Revision,
            TenureMonths,
            Minimum,
            CostCenter,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
