namespace Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional payslip exception is in its life cycle.</summary>
public enum RegionalPayslipExceptionStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A regional payslip exception. Its code is unique within a tenant.</summary>
public sealed class RegionalPayslipException(string code, string label, string title, int expiresAt, int updatedAt, RegionalPayslipExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public int UpdatedAt { get; set; } = updatedAt;
    public RegionalPayslipExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a regional payslip exception in status draft.</summary>
    public static RegionalPayslipException NewRegionalPayslipException(string code, string label)
    {
        return new RegionalPayslipException(code, label, "", 0, 0, RegionalPayslipExceptionStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalPayslipExceptionStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalPayslipExceptionStatus next)
    {
        switch (Status)
        {
            case RegionalPayslipExceptionStatus.Draft:
                return next == RegionalPayslipExceptionStatus.Active || next == RegionalPayslipExceptionStatus.Suspended;
            case RegionalPayslipExceptionStatus.Pending:
                return next == RegionalPayslipExceptionStatus.Suspended;
            case RegionalPayslipExceptionStatus.Active:
                switch (next)
                {
                    case RegionalPayslipExceptionStatus.Closed:
                        return true;
                }
                return false;
            case RegionalPayslipExceptionStatus.Suspended:
                return next == RegionalPayslipExceptionStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RegionalPayslipExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional payslip exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"regional payslip exception"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)UpdatedAt * (double)ExpiresAt, 2);
    }

    public RegionalPayslipException Copy()
    {
        return new RegionalPayslipException(Code, Label, Title, ExpiresAt, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
