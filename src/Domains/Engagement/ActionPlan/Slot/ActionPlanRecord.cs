namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan record is in its life cycle.</summary>
public enum ActionPlanRecordStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a action plan record and its current status.</summary>
public sealed class ActionPlanRecord(string code, string name, string label, int attempts, int endsAt, int batchSize, double fte, ActionPlanRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public int Attempts { get; set; } = attempts;
    public int EndsAt { get; set; } = endsAt;
    public int BatchSize { get; set; } = batchSize;
    public double Fte { get; set; } = fte;
    public ActionPlanRecordStatus Status { get; set; } = status;

    /// <summary>Starts a action plan record in status planned.</summary>
    public static ActionPlanRecord NewActionPlanRecord(string code, string name)
    {
        return new ActionPlanRecord(code, name, "", 0, 0, 0, 0.0, ActionPlanRecordStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanRecordStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanRecordStatus next)
    {
        switch (Status)
        {
            case ActionPlanRecordStatus.Planned:
                switch (next)
                {
                    case ActionPlanRecordStatus.InProgress:
                        return true;
                }
                return false;
            case ActionPlanRecordStatus.Scheduled:
                switch (next)
                {
                    case ActionPlanRecordStatus.InProgress:
                        return true;
                }
                return false;
            case ActionPlanRecordStatus.InProgress:
                return next == ActionPlanRecordStatus.Completed || next == ActionPlanRecordStatus.Cancelled;
            case ActionPlanRecordStatus.Completed:
                return next == ActionPlanRecordStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ActionPlanRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"action plan record"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Fte + (double)Attempts, 2);
    }

    public ActionPlanRecord Copy()
    {
        return new ActionPlanRecord(Code, Name, Label, Attempts, EndsAt, BatchSize, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
