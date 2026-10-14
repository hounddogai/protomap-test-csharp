namespace Acme.Workforce.Workplace.Visitor.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor profile is in its life cycle.</summary>
public enum VisitorProfileStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a visitor profile and its current status.</summary>
public sealed class VisitorProfile(string code, string label, string badge, string timezone, int revision, int priority, VisitorProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public string Timezone { get; set; } = timezone;
    public int Revision { get; set; } = revision;
    public int Priority { get; set; } = priority;
    public VisitorProfileStatus Status { get; set; } = status;

    /// <summary>Starts a visitor profile in status new.</summary>
    public static VisitorProfile NewVisitorProfile(string code, string label)
    {
        return new VisitorProfile(code, label, "", "", 0, 0, VisitorProfileStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorProfileStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorProfileStatus next)
    {
        switch (Status)
        {
            case VisitorProfileStatus.New:
                switch (next)
                {
                    case VisitorProfileStatus.Running:
                        return true;
                }
                return false;
            case VisitorProfileStatus.Queued:
                return next == VisitorProfileStatus.Done || next == VisitorProfileStatus.Failed;
            case VisitorProfileStatus.Running:
                switch (next)
                {
                    case VisitorProfileStatus.Done:
                        return true;
                }
                return false;
            case VisitorProfileStatus.Done:
                return next == VisitorProfileStatus.Failed;
        }
        return false;
    }

    public void MoveTo(VisitorProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"visitor profile"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Revision + (double)Priority, 2);
    }

    public VisitorProfile Copy()
    {
        return new VisitorProfile(Code, Label, Badge, Timezone, Revision, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
