namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor rollup is in its life cycle.</summary>
public enum VisitorRollupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A visitor rollup as the visitor team keeps it.</summary>
public sealed class VisitorRollup(string code, string name, string unit, string title, string notes, int expiresAt, VisitorRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public int ExpiresAt { get; set; } = expiresAt;
    public VisitorRollupStatus Status { get; set; } = status;

    /// <summary>Starts a visitor rollup in status new.</summary>
    public static VisitorRollup NewVisitorRollup(string code, string name)
    {
        return new VisitorRollup(code, name, "", "", "", 0, VisitorRollupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorRollupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorRollupStatus next)
    {
        switch (Status)
        {
            case VisitorRollupStatus.New:
                switch (next)
                {
                    case VisitorRollupStatus.Done:
                        return true;
                }
                return false;
            case VisitorRollupStatus.Queued:
                switch (next)
                {
                    case VisitorRollupStatus.Done:
                        return true;
                }
                return false;
            case VisitorRollupStatus.Running:
                return next == VisitorRollupStatus.Failed;
            case VisitorRollupStatus.Done:
                return next == VisitorRollupStatus.Failed;
        }
        return false;
    }

    public void MoveTo(VisitorRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public VisitorRollup Copy()
    {
        return new VisitorRollup(Code, Name, Unit, Title, Notes, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
