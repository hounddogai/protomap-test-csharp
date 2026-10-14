namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception budget is in its life cycle.</summary>
public enum ExceptionBudgetStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A exception budget as the exception team keeps it.</summary>
public sealed class ExceptionBudget(string code, string name, int windowDays, int startsAt, string segment, ExceptionBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public int StartsAt { get; set; } = startsAt;
    public string Segment { get; set; } = segment;
    public ExceptionBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a exception budget in status proposed.</summary>
    public static ExceptionBudget NewExceptionBudget(string code, string name)
    {
        return new ExceptionBudget(code, name, 0, 0, "", ExceptionBudgetStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionBudgetStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionBudgetStatus next)
    {
        switch (Status)
        {
            case ExceptionBudgetStatus.Proposed:
                switch (next)
                {
                    case ExceptionBudgetStatus.Rejected:
                    case ExceptionBudgetStatus.Withdrawn:
                        return true;
                }
                return false;
            case ExceptionBudgetStatus.Approved:
                switch (next)
                {
                    case ExceptionBudgetStatus.Rejected:
                        return true;
                }
                return false;
            case ExceptionBudgetStatus.Rejected:
                return next == ExceptionBudgetStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ExceptionBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetVolume()
    {
        return Math.Round((double)StartsAt + (double)WindowDays, 2);
    }

    public ExceptionBudget Copy()
    {
        return new ExceptionBudget(Code, Name, WindowDays, StartsAt, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
