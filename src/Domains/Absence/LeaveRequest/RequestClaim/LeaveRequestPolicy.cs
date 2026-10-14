namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request policy is in its life cycle.</summary>
public enum LeaveRequestPolicyStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A leave request policy as the leave request team keeps it.</summary>
public sealed class LeaveRequestPolicy(string code, string label, double ratio, double budget, string notes, double floorValue, LeaveRequestPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public double Budget { get; set; } = budget;
    public string Notes { get; set; } = notes;
    public double FloorValue { get; set; } = floorValue;
    public LeaveRequestPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a leave request policy in status new.</summary>
    public static LeaveRequestPolicy NewLeaveRequestPolicy(string code, string label)
    {
        return new LeaveRequestPolicy(code, label, 0.0, 0.0, "", 0.0, LeaveRequestPolicyStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestPolicyStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestPolicyStatus next)
    {
        switch (Status)
        {
            case LeaveRequestPolicyStatus.New:
                return next == LeaveRequestPolicyStatus.Running;
            case LeaveRequestPolicyStatus.Queued:
                switch (next)
                {
                    case LeaveRequestPolicyStatus.Running:
                    case LeaveRequestPolicyStatus.Done:
                        return true;
                }
                return false;
            case LeaveRequestPolicyStatus.Running:
                return next == LeaveRequestPolicyStatus.Done;
            case LeaveRequestPolicyStatus.Done:
                return next == LeaveRequestPolicyStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LeaveRequestPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetTotal()
    {
        return Math.Round(Ratio - FloorValue, 2);
    }

    public LeaveRequestPolicy Copy()
    {
        return new LeaveRequestPolicy(Code, Label, Ratio, Budget, Notes, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
