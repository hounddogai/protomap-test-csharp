namespace Acme.Workforce.Absence.ReturnPlan.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending return plan window is in its life cycle.</summary>
public enum PendingReturnPlanWindowStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a pending return plan window and its current status.</summary>
public sealed class PendingReturnPlanWindow(string code, string label, int sequence, string notes, string phone, PendingReturnPlanWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public string Notes { get; set; } = notes;
    public string Phone { get; set; } = phone;
    public PendingReturnPlanWindowStatus Status { get; set; } = status;

    /// <summary>Starts a pending return plan window in status idle.</summary>
    public static PendingReturnPlanWindow NewPendingReturnPlanWindow(string code, string label)
    {
        return new PendingReturnPlanWindow(code, label, 0, "", "", PendingReturnPlanWindowStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingReturnPlanWindowStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingReturnPlanWindowStatus next)
    {
        switch (Status)
        {
            case PendingReturnPlanWindowStatus.Idle:
                return next == PendingReturnPlanWindowStatus.Stopped;
            case PendingReturnPlanWindowStatus.Warming:
                switch (next)
                {
                    case PendingReturnPlanWindowStatus.Draining:
                        return true;
                }
                return false;
            case PendingReturnPlanWindowStatus.Ready:
                return next == PendingReturnPlanWindowStatus.Stopped;
            case PendingReturnPlanWindowStatus.Draining:
                switch (next)
                {
                    case PendingReturnPlanWindowStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingReturnPlanWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending return plan window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return (double)Sequence;
    }

    public PendingReturnPlanWindow Copy()
    {
        return new PendingReturnPlanWindow(Code, Label, Sequence, Notes, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
