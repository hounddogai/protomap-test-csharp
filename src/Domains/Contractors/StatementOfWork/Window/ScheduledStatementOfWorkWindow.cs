namespace Acme.Workforce.Contractors.StatementOfWork.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled statement of work window is in its life cycle.</summary>
public enum ScheduledStatementOfWorkWindowStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A scheduled statement of work window. Its code is unique within a tenant.</summary>
public sealed class ScheduledStatementOfWorkWindow(string code, string label, string country, bool active, double rating, ScheduledStatementOfWorkWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public bool Active { get; set; } = active;
    public double Rating { get; set; } = rating;
    public ScheduledStatementOfWorkWindowStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled statement of work window in status idle.</summary>
    public static ScheduledStatementOfWorkWindow NewScheduledStatementOfWorkWindow(string code, string label)
    {
        return new ScheduledStatementOfWorkWindow(
            code,
            label,
            "",
            false,
            0.0,
            ScheduledStatementOfWorkWindowStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledStatementOfWorkWindowStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledStatementOfWorkWindowStatus next)
    {
        switch (Status)
        {
            case ScheduledStatementOfWorkWindowStatus.Idle:
                return next == ScheduledStatementOfWorkWindowStatus.Draining || next == ScheduledStatementOfWorkWindowStatus.Stopped;
            case ScheduledStatementOfWorkWindowStatus.Warming:
                switch (next)
                {
                    case ScheduledStatementOfWorkWindowStatus.Ready:
                    case ScheduledStatementOfWorkWindowStatus.Stopped:
                        return true;
                }
                return false;
            case ScheduledStatementOfWorkWindowStatus.Ready:
                return next == ScheduledStatementOfWorkWindowStatus.Draining;
            case ScheduledStatementOfWorkWindowStatus.Draining:
                return next == ScheduledStatementOfWorkWindowStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ScheduledStatementOfWorkWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled statement of work window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return Rating;
    }

    public ScheduledStatementOfWorkWindow Copy()
    {
        return new ScheduledStatementOfWorkWindow(Code, Label, Country, Active, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
