namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction notice is in its life cycle.</summary>
public enum DeductionNoticeStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One deduction notice, with what the planners need to act on it.</summary>
public sealed class DeductionNotice(string code, string label, string title, string costCenter, double fte, int quantity, bool visible, double cost, DeductionNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public double Fte { get; set; } = fte;
    public int Quantity { get; set; } = quantity;
    public bool Visible { get; set; } = visible;
    public double Cost { get; set; } = cost;
    public DeductionNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a deduction notice in status proposed.</summary>
    public static DeductionNotice NewDeductionNotice(string code, string label)
    {
        return new DeductionNotice(code, label, "", "", 0.0, 0, false, 0.0, DeductionNoticeStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionNoticeStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionNoticeStatus next)
    {
        switch (Status)
        {
            case DeductionNoticeStatus.Proposed:
                switch (next)
                {
                    case DeductionNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
            case DeductionNoticeStatus.Approved:
                switch (next)
                {
                    case DeductionNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
            case DeductionNoticeStatus.Rejected:
                return next == DeductionNoticeStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DeductionNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Quantity + Cost, 2);
    }

    public DeductionNotice Copy()
    {
        return new DeductionNotice(Code, Label, Title, CostCenter, Fte, Quantity, Visible, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
