namespace Acme.Workforce.Compensation.EquityGrant.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant entry is in its life cycle.</summary>
public enum EquityGrantEntryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One equity grant entry, with what the planners need to act on it.</summary>
public sealed class EquityGrantEntry(string code, string label, double hours, double budget, int revision, EquityGrantEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public double Budget { get; set; } = budget;
    public int Revision { get; set; } = revision;
    public EquityGrantEntryStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant entry in status idle.</summary>
    public static EquityGrantEntry NewEquityGrantEntry(string code, string label)
    {
        return new EquityGrantEntry(code, label, 0.0, 0.0, 0, EquityGrantEntryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantEntryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantEntryStatus next)
    {
        switch (Status)
        {
            case EquityGrantEntryStatus.Idle:
                switch (next)
                {
                    case EquityGrantEntryStatus.Draining:
                    case EquityGrantEntryStatus.Stopped:
                        return true;
                }
                return false;
            case EquityGrantEntryStatus.Warming:
                return next == EquityGrantEntryStatus.Ready;
            case EquityGrantEntryStatus.Ready:
                switch (next)
                {
                    case EquityGrantEntryStatus.Stopped:
                        return true;
                }
                return false;
            case EquityGrantEntryStatus.Draining:
                return next == EquityGrantEntryStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(EquityGrantEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetLoad()
    {
        return Math.Round((double)Revision + Budget, 2);
    }

    public EquityGrantEntry Copy()
    {
        return new EquityGrantEntry(Code, Label, Hours, Budget, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
