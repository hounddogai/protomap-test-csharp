namespace Acme.Workforce.Compensation.BonusPool.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool batch is in its life cycle.</summary>
public enum BonusPoolBatchStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A bonus pool batch as the bonus pool team keeps it.</summary>
public sealed class BonusPoolBatch(string code, string title, string segment, string source, string name, int revision, string channel, string employeeId, string label, BonusPoolBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Segment { get; set; } = segment;
    public string Source { get; set; } = source;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public string Channel { get; set; } = channel;
    public string EmployeeId { get; set; } = employeeId;
    public string Label { get; set; } = label;
    public BonusPoolBatchStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool batch in status new.</summary>
    public static BonusPoolBatch NewBonusPoolBatch(string code, string title)
    {
        return new BonusPoolBatch(code, title, "", "", "", 0, "", "", "", BonusPoolBatchStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolBatchStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolBatchStatus next)
    {
        switch (Status)
        {
            case BonusPoolBatchStatus.New:
                switch (next)
                {
                    case BonusPoolBatchStatus.Failed:
                        return true;
                }
                return false;
            case BonusPoolBatchStatus.Queued:
                return next == BonusPoolBatchStatus.Running || next == BonusPoolBatchStatus.Done;
            case BonusPoolBatchStatus.Running:
                switch (next)
                {
                    case BonusPoolBatchStatus.Done:
                    case BonusPoolBatchStatus.Failed:
                        return true;
                }
                return false;
            case BonusPoolBatchStatus.Done:
                switch (next)
                {
                    case BonusPoolBatchStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Revision;
    }

    public BonusPoolBatch Copy()
    {
        return new BonusPoolBatch(Code, Title, Segment, Source, Name, Revision, Channel, EmployeeId, Label, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
