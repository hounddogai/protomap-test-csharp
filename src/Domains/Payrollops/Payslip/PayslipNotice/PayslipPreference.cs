namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip preference is in its life cycle.</summary>
public enum PayslipPreferenceStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A payslip preference. Its code is unique within a tenant.</summary>
public sealed class PayslipPreference(string code, string name, int capacity, string title, int revision, double maximum, double weight, bool pinned, PayslipPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public double Maximum { get; set; } = maximum;
    public double Weight { get; set; } = weight;
    public bool Pinned { get; set; } = pinned;
    public PayslipPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a payslip preference in status requested.</summary>
    public static PayslipPreference NewPayslipPreference(string code, string name)
    {
        return new PayslipPreference(code, name, 0, "", 0, 0.0, 0.0, false, PayslipPreferenceStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipPreferenceStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipPreferenceStatus next)
    {
        switch (Status)
        {
            case PayslipPreferenceStatus.Requested:
                return next == PayslipPreferenceStatus.Granted || next == PayslipPreferenceStatus.Expired;
            case PayslipPreferenceStatus.Granted:
                return next == PayslipPreferenceStatus.Revoked || next == PayslipPreferenceStatus.Expired;
            case PayslipPreferenceStatus.Revoked:
                switch (next)
                {
                    case PayslipPreferenceStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayslipPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Capacity - Maximum, 2);
    }

    public PayslipPreference Copy()
    {
        return new PayslipPreference(Code, Name, Capacity, Title, Revision, Maximum, Weight, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
