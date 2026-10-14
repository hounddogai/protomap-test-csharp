namespace Acme.Workforce.Compensation.PayBand.BandCycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped pay band quota is in its life cycle.</summary>
public enum CappedPayBandQuotaStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A capped pay band quota. Its code is unique within a tenant.</summary>
public sealed class CappedPayBandQuota(string code, string title, double floorValue, string department, string employeeId, CappedPayBandQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public string Department { get; set; } = department;
    public string EmployeeId { get; set; } = employeeId;
    public CappedPayBandQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a capped pay band quota in status new.</summary>
    public static CappedPayBandQuota NewCappedPayBandQuota(string code, string title)
    {
        return new CappedPayBandQuota(code, title, 0.0, "", "", CappedPayBandQuotaStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedPayBandQuotaStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedPayBandQuotaStatus next)
    {
        switch (Status)
        {
            case CappedPayBandQuotaStatus.New:
                return next == CappedPayBandQuotaStatus.Queued;
            case CappedPayBandQuotaStatus.Queued:
                return next == CappedPayBandQuotaStatus.Running || next == CappedPayBandQuotaStatus.Done;
            case CappedPayBandQuotaStatus.Running:
                return next == CappedPayBandQuotaStatus.Done;
            case CappedPayBandQuotaStatus.Done:
                return next == CappedPayBandQuotaStatus.Failed;
        }
        return false;
    }

    public void MoveTo(CappedPayBandQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped pay band quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return FloorValue;
    }

    public CappedPayBandQuota Copy()
    {
        return new CappedPayBandQuota(Code, Title, FloorValue, Department, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
