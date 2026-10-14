namespace Acme.Workforce.Absence.Accrual.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual cycle is in its life cycle.</summary>
public enum AccrualCycleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One accrual cycle, with what the planners need to act on it.</summary>
public sealed class AccrualCycle(string code, string name, int createdAt, bool approved, string skill, int retries, string category, double budget, bool active, AccrualCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public bool Approved { get; set; } = approved;
    public string Skill { get; set; } = skill;
    public int Retries { get; set; } = retries;
    public string Category { get; set; } = category;
    public double Budget { get; set; } = budget;
    public bool Active { get; set; } = active;
    public AccrualCycleStatus Status { get; set; } = status;

    /// <summary>Starts a accrual cycle in status new.</summary>
    public static AccrualCycle NewAccrualCycle(string code, string name)
    {
        return new AccrualCycle(code, name, 0, false, "", 0, "", 0.0, false, AccrualCycleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualCycleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualCycleStatus next)
    {
        switch (Status)
        {
            case AccrualCycleStatus.New:
                switch (next)
                {
                    case AccrualCycleStatus.Running:
                    case AccrualCycleStatus.Done:
                        return true;
                }
                return false;
            case AccrualCycleStatus.Queued:
                return next == AccrualCycleStatus.Running || next == AccrualCycleStatus.Failed;
            case AccrualCycleStatus.Running:
                switch (next)
                {
                    case AccrualCycleStatus.Done:
                        return true;
                }
                return false;
            case AccrualCycleStatus.Done:
                switch (next)
                {
                    case AccrualCycleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccrualCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetVolume()
    {
        return Math.Round((double)Retries * Budget, 2);
    }

    public AccrualCycle Copy()
    {
        return new AccrualCycle(Code, Name, CreatedAt, Approved, Skill, Retries, Category, Budget, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
