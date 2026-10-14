namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled position template is in its life cycle.</summary>
public enum ScheduledPositionTemplateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A scheduled position template as the position team keeps it.</summary>
public sealed class ScheduledPositionTemplate(string code, string title, string summary, int retries, string unit, ScheduledPositionTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public int Retries { get; set; } = retries;
    public string Unit { get; set; } = unit;
    public ScheduledPositionTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled position template in status idle.</summary>
    public static ScheduledPositionTemplate NewScheduledPositionTemplate(string code, string title)
    {
        return new ScheduledPositionTemplate(code, title, "", 0, "", ScheduledPositionTemplateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPositionTemplateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPositionTemplateStatus next)
    {
        switch (Status)
        {
            case ScheduledPositionTemplateStatus.Idle:
                return next == ScheduledPositionTemplateStatus.Ready || next == ScheduledPositionTemplateStatus.Stopped;
            case ScheduledPositionTemplateStatus.Warming:
                return next == ScheduledPositionTemplateStatus.Ready;
            case ScheduledPositionTemplateStatus.Ready:
                return next == ScheduledPositionTemplateStatus.Stopped;
            case ScheduledPositionTemplateStatus.Draining:
                return next == ScheduledPositionTemplateStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ScheduledPositionTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled position template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled position template"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public ScheduledPositionTemplate Copy()
    {
        return new ScheduledPositionTemplate(Code, Title, Summary, Retries, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
