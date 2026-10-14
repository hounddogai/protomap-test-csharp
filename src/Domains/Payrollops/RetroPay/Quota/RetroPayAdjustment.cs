namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay adjustment is in its life cycle.</summary>
public enum RetroPayAdjustmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One retro pay adjustment, with what the planners need to act on it.</summary>
public sealed class RetroPayAdjustment(string code, string label, int leaveDays, int revision, string reviewer, string costCenter, RetroPayAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public int Revision { get; set; } = revision;
    public string Reviewer { get; set; } = reviewer;
    public string CostCenter { get; set; } = costCenter;
    public RetroPayAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay adjustment in status proposed.</summary>
    public static RetroPayAdjustment NewRetroPayAdjustment(string code, string label)
    {
        return new RetroPayAdjustment(code, label, 0, 0, "", "", RetroPayAdjustmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayAdjustmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayAdjustmentStatus next)
    {
        switch (Status)
        {
            case RetroPayAdjustmentStatus.Proposed:
                return next == RetroPayAdjustmentStatus.Approved || next == RetroPayAdjustmentStatus.Withdrawn;
            case RetroPayAdjustmentStatus.Approved:
                return next == RetroPayAdjustmentStatus.Withdrawn;
            case RetroPayAdjustmentStatus.Rejected:
                switch (next)
                {
                    case RetroPayAdjustmentStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RetroPayAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)LeaveDays + (double)Revision, 2);
    }

    public RetroPayAdjustment Copy()
    {
        return new RetroPayAdjustment(Code, Label, LeaveDays, Revision, Reviewer, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
