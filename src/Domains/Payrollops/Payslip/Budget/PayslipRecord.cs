namespace Acme.Workforce.Payrollops.Payslip.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip record is in its life cycle.</summary>
public enum PayslipRecordStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a payslip record and its current status.</summary>
public sealed class PayslipRecord(string code, string label, int expiresAt, int createdAt, bool approved, PayslipRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public int CreatedAt { get; set; } = createdAt;
    public bool Approved { get; set; } = approved;
    public PayslipRecordStatus Status { get; set; } = status;

    /// <summary>Starts a payslip record in status requested.</summary>
    public static PayslipRecord NewPayslipRecord(string code, string label)
    {
        return new PayslipRecord(code, label, 0, 0, false, PayslipRecordStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipRecordStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipRecordStatus next)
    {
        switch (Status)
        {
            case PayslipRecordStatus.Requested:
                switch (next)
                {
                    case PayslipRecordStatus.Revoked:
                    case PayslipRecordStatus.Expired:
                        return true;
                }
                return false;
            case PayslipRecordStatus.Granted:
                return next == PayslipRecordStatus.Revoked || next == PayslipRecordStatus.Expired;
            case PayslipRecordStatus.Revoked:
                return next == PayslipRecordStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PayslipRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"payslip record"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)CreatedAt * (double)ExpiresAt, 2);
    }

    public PayslipRecord Copy()
    {
        return new PayslipRecord(Code, Label, ExpiresAt, CreatedAt, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
