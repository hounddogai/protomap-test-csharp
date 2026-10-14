namespace Acme.Workforce.Payrollops.Payslip.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip mapping is in its life cycle.</summary>
public enum PayslipMappingStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A payslip mapping as the payslip team keeps it.</summary>
public sealed class PayslipMapping(string code, string label, int expiresAt, bool archived, string title, bool active, PayslipMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Archived { get; set; } = archived;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public PayslipMappingStatus Status { get; set; } = status;

    /// <summary>Starts a payslip mapping in status new.</summary>
    public static PayslipMapping NewPayslipMapping(string code, string label)
    {
        return new PayslipMapping(code, label, 0, false, "", false, PayslipMappingStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipMappingStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipMappingStatus next)
    {
        switch (Status)
        {
            case PayslipMappingStatus.New:
                switch (next)
                {
                    case PayslipMappingStatus.Done:
                        return true;
                }
                return false;
            case PayslipMappingStatus.Queued:
                return next == PayslipMappingStatus.Running;
            case PayslipMappingStatus.Running:
                return next == PayslipMappingStatus.Done;
            case PayslipMappingStatus.Done:
                switch (next)
                {
                    case PayslipMappingStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayslipMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public PayslipMapping Copy()
    {
        return new PayslipMapping(Code, Label, ExpiresAt, Archived, Title, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
