namespace Acme.Workforce.Workplace.Desk.DeskProfile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk cycle is in its life cycle.</summary>
public enum DeskCycleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a desk cycle and its current status.</summary>
public sealed class DeskCycle(string code, string title, string managerId, string timezone, string skill, bool archived, double amount, DeskCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public string Timezone { get; set; } = timezone;
    public string Skill { get; set; } = skill;
    public bool Archived { get; set; } = archived;
    public double Amount { get; set; } = amount;
    public DeskCycleStatus Status { get; set; } = status;

    /// <summary>Starts a desk cycle in status new.</summary>
    public static DeskCycle NewDeskCycle(string code, string title)
    {
        return new DeskCycle(code, title, "", "", "", false, 0.0, DeskCycleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskCycleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskCycleStatus next)
    {
        switch (Status)
        {
            case DeskCycleStatus.New:
                return next == DeskCycleStatus.Done;
            case DeskCycleStatus.Queued:
                return next == DeskCycleStatus.Running || next == DeskCycleStatus.Failed;
            case DeskCycleStatus.Running:
                return next == DeskCycleStatus.Failed;
            case DeskCycleStatus.Done:
                return next == DeskCycleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(DeskCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public DeskCycle Copy()
    {
        return new DeskCycle(Code, Title, ManagerId, Timezone, Skill, Archived, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
