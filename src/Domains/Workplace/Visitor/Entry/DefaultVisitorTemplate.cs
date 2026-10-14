namespace Acme.Workforce.Workplace.Visitor.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default visitor template is in its life cycle.</summary>
public enum DefaultVisitorTemplateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One default visitor template, with what the planners need to act on it.</summary>
public sealed class DefaultVisitorTemplate(string code, string name, int priority, bool archived, string country, int sequence, DefaultVisitorTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public bool Archived { get; set; } = archived;
    public string Country { get; set; } = country;
    public int Sequence { get; set; } = sequence;
    public DefaultVisitorTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a default visitor template in status planned.</summary>
    public static DefaultVisitorTemplate NewDefaultVisitorTemplate(string code, string name)
    {
        return new DefaultVisitorTemplate(code, name, 0, false, "", 0, DefaultVisitorTemplateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultVisitorTemplateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultVisitorTemplateStatus next)
    {
        switch (Status)
        {
            case DefaultVisitorTemplateStatus.Planned:
                switch (next)
                {
                    case DefaultVisitorTemplateStatus.Cancelled:
                        return true;
                }
                return false;
            case DefaultVisitorTemplateStatus.Scheduled:
                switch (next)
                {
                    case DefaultVisitorTemplateStatus.InProgress:
                        return true;
                }
                return false;
            case DefaultVisitorTemplateStatus.InProgress:
                return next == DefaultVisitorTemplateStatus.Completed || next == DefaultVisitorTemplateStatus.Cancelled;
            case DefaultVisitorTemplateStatus.Completed:
                return next == DefaultVisitorTemplateStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(DefaultVisitorTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default visitor template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Sequence - (double)Priority, 2);
    }

    public DefaultVisitorTemplate Copy()
    {
        return new DefaultVisitorTemplate(Code, Name, Priority, Archived, Country, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
