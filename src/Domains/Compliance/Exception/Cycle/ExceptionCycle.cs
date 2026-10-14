namespace Acme.Workforce.Compliance.Exception.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception cycle is in its life cycle.</summary>
public enum ExceptionCycleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A exception cycle as the exception team keeps it.</summary>
public sealed class ExceptionCycle(string code, string label, double maximum, int retries, int createdAt, int windowDays, ExceptionCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public int Retries { get; set; } = retries;
    public int CreatedAt { get; set; } = createdAt;
    public int WindowDays { get; set; } = windowDays;
    public ExceptionCycleStatus Status { get; set; } = status;

    /// <summary>Starts a exception cycle in status proposed.</summary>
    public static ExceptionCycle NewExceptionCycle(string code, string label)
    {
        return new ExceptionCycle(code, label, 0.0, 0, 0, 0, ExceptionCycleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionCycleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionCycleStatus next)
    {
        switch (Status)
        {
            case ExceptionCycleStatus.Proposed:
                switch (next)
                {
                    case ExceptionCycleStatus.Approved:
                    case ExceptionCycleStatus.Withdrawn:
                        return true;
                }
                return false;
            case ExceptionCycleStatus.Approved:
                switch (next)
                {
                    case ExceptionCycleStatus.Rejected:
                    case ExceptionCycleStatus.Withdrawn:
                        return true;
                }
                return false;
            case ExceptionCycleStatus.Rejected:
                switch (next)
                {
                    case ExceptionCycleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ExceptionCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"exception cycle"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Maximum * (double)CreatedAt, 2);
    }

    public ExceptionCycle Copy()
    {
        return new ExceptionCycle(Code, Label, Maximum, Retries, CreatedAt, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
