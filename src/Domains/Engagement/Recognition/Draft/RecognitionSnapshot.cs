namespace Acme.Workforce.Engagement.Recognition.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition snapshot is in its life cycle.</summary>
public enum RecognitionSnapshotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A recognition snapshot as the recognition team keeps it.</summary>
public sealed class RecognitionSnapshot(string code, string label, string salaryBand, int sequence, string source, string name, RecognitionSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public int Sequence { get; set; } = sequence;
    public string Source { get; set; } = source;
    public string Name { get; set; } = name;
    public RecognitionSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a recognition snapshot in status new.</summary>
    public static RecognitionSnapshot NewRecognitionSnapshot(string code, string label)
    {
        return new RecognitionSnapshot(code, label, "", 0, "", "", RecognitionSnapshotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionSnapshotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionSnapshotStatus next)
    {
        switch (Status)
        {
            case RecognitionSnapshotStatus.New:
                return next == RecognitionSnapshotStatus.Done || next == RecognitionSnapshotStatus.Failed;
            case RecognitionSnapshotStatus.Queued:
                return next == RecognitionSnapshotStatus.Done || next == RecognitionSnapshotStatus.Failed;
            case RecognitionSnapshotStatus.Running:
                return next == RecognitionSnapshotStatus.Done || next == RecognitionSnapshotStatus.Failed;
            case RecognitionSnapshotStatus.Done:
                switch (next)
                {
                    case RecognitionSnapshotStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RecognitionSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return (double)Sequence;
    }

    public RecognitionSnapshot Copy()
    {
        return new RecognitionSnapshot(Code, Label, SalaryBand, Sequence, Source, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
