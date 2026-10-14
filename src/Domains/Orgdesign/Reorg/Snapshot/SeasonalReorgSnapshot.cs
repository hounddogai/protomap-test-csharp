namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal reorg snapshot is in its life cycle.</summary>
public enum SeasonalReorgSnapshotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A seasonal reorg snapshot. Its code is unique within a tenant.</summary>
public sealed class SeasonalReorgSnapshot(string code, string label, string employeeId, string location, double ratio, double rating, double rate, double amount, int revision, SeasonalReorgSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public string Location { get; set; } = location;
    public double Ratio { get; set; } = ratio;
    public double Rating { get; set; } = rating;
    public double Rate { get; set; } = rate;
    public double Amount { get; set; } = amount;
    public int Revision { get; set; } = revision;
    public SeasonalReorgSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal reorg snapshot in status new.</summary>
    public static SeasonalReorgSnapshot NewSeasonalReorgSnapshot(string code, string label)
    {
        return new SeasonalReorgSnapshot(
            code,
            label,
            "",
            "",
            0.0,
            0.0,
            0.0,
            0.0,
            0,
            SeasonalReorgSnapshotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalReorgSnapshotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalReorgSnapshotStatus next)
    {
        switch (Status)
        {
            case SeasonalReorgSnapshotStatus.New:
                return next == SeasonalReorgSnapshotStatus.Queued || next == SeasonalReorgSnapshotStatus.Done;
            case SeasonalReorgSnapshotStatus.Queued:
                switch (next)
                {
                    case SeasonalReorgSnapshotStatus.Running:
                    case SeasonalReorgSnapshotStatus.Failed:
                        return true;
                }
                return false;
            case SeasonalReorgSnapshotStatus.Running:
                switch (next)
                {
                    case SeasonalReorgSnapshotStatus.Failed:
                        return true;
                }
                return false;
            case SeasonalReorgSnapshotStatus.Done:
                return next == SeasonalReorgSnapshotStatus.Failed;
        }
        return false;
    }

    public void MoveTo(SeasonalReorgSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal reorg snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"seasonal reorg snapshot"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Revision * Amount, 2);
    }

    public SeasonalReorgSnapshot Copy()
    {
        return new SeasonalReorgSnapshot(
            Code,
            Label,
            EmployeeId,
            Location,
            Ratio,
            Rating,
            Rate,
            Amount,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
