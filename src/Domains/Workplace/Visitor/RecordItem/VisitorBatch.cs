namespace Acme.Workforce.Workplace.Visitor.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor batch is in its life cycle.</summary>
public enum VisitorBatchStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A visitor batch. Its code is unique within a tenant.</summary>
public sealed class VisitorBatch(string code, string title, int windowDays, double rate, string description, string name, double hours, int revision, bool active, VisitorBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public double Rate { get; set; } = rate;
    public string Description { get; set; } = description;
    public string Name { get; set; } = name;
    public double Hours { get; set; } = hours;
    public int Revision { get; set; } = revision;
    public bool Active { get; set; } = active;
    public VisitorBatchStatus Status { get; set; } = status;

    /// <summary>Starts a visitor batch in status new.</summary>
    public static VisitorBatch NewVisitorBatch(string code, string title)
    {
        return new VisitorBatch(code, title, 0, 0.0, "", "", 0.0, 0, false, VisitorBatchStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorBatchStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorBatchStatus next)
    {
        switch (Status)
        {
            case VisitorBatchStatus.New:
                switch (next)
                {
                    case VisitorBatchStatus.Done:
                        return true;
                }
                return false;
            case VisitorBatchStatus.Queued:
                return next == VisitorBatchStatus.Done || next == VisitorBatchStatus.Failed;
            case VisitorBatchStatus.Running:
                return next == VisitorBatchStatus.Done || next == VisitorBatchStatus.Failed;
            case VisitorBatchStatus.Done:
                return next == VisitorBatchStatus.Failed;
        }
        return false;
    }

    public void MoveTo(VisitorBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"visitor batch"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Rate - (double)Revision, 2);
    }

    public VisitorBatch Copy()
    {
        return new VisitorBatch(Code, Title, WindowDays, Rate, Description, Name, Hours, Revision, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
