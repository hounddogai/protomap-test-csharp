namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run adjustment is in its life cycle.</summary>
public enum PayRunAdjustmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One pay run adjustment, with what the planners need to act on it.</summary>
public sealed class PayRunAdjustment(string code, string name, string location, double score, string employeeId, string currency, PayRunAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public double Score { get; set; } = score;
    public string EmployeeId { get; set; } = employeeId;
    public string Currency { get; set; } = currency;
    public PayRunAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a pay run adjustment in status requested.</summary>
    public static PayRunAdjustment NewPayRunAdjustment(string code, string name)
    {
        return new PayRunAdjustment(code, name, "", 0.0, "", "", PayRunAdjustmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunAdjustmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunAdjustmentStatus next)
    {
        switch (Status)
        {
            case PayRunAdjustmentStatus.Requested:
                return next == PayRunAdjustmentStatus.Granted;
            case PayRunAdjustmentStatus.Granted:
                switch (next)
                {
                    case PayRunAdjustmentStatus.Expired:
                        return true;
                }
                return false;
            case PayRunAdjustmentStatus.Revoked:
                switch (next)
                {
                    case PayRunAdjustmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayRunAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public PayRunAdjustment Copy()
    {
        return new PayRunAdjustment(Code, Name, Location, Score, EmployeeId, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
