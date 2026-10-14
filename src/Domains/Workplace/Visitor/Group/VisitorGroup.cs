namespace Acme.Workforce.Workplace.Visitor.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor group is in its life cycle.</summary>
public enum VisitorGroupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a visitor group and its current status.</summary>
public sealed class VisitorGroup(string code, string title, string badge, double hours, string grade, VisitorGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public double Hours { get; set; } = hours;
    public string Grade { get; set; } = grade;
    public VisitorGroupStatus Status { get; set; } = status;

    /// <summary>Starts a visitor group in status proposed.</summary>
    public static VisitorGroup NewVisitorGroup(string code, string title)
    {
        return new VisitorGroup(code, title, "", 0.0, "", VisitorGroupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorGroupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorGroupStatus next)
    {
        switch (Status)
        {
            case VisitorGroupStatus.Proposed:
                switch (next)
                {
                    case VisitorGroupStatus.Withdrawn:
                        return true;
                }
                return false;
            case VisitorGroupStatus.Approved:
                switch (next)
                {
                    case VisitorGroupStatus.Rejected:
                        return true;
                }
                return false;
            case VisitorGroupStatus.Rejected:
                return next == VisitorGroupStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(VisitorGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Hours;
    }

    public VisitorGroup Copy()
    {
        return new VisitorGroup(Code, Title, Badge, Hours, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
