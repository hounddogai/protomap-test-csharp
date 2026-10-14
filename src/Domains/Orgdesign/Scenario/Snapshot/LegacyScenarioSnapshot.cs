namespace Acme.Workforce.Orgdesign.Scenario.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy scenario snapshot is in its life cycle.</summary>
public enum LegacyScenarioSnapshotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One legacy scenario snapshot, with what the planners need to act on it.</summary>
public sealed class LegacyScenarioSnapshot(string code, string label, string reviewer, string title, int graceDays, string channel, int windowDays, string costCenter, int tenureMonths, double rating, int batchSize, LegacyScenarioSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public string Title { get; set; } = title;
    public int GraceDays { get; set; } = graceDays;
    public string Channel { get; set; } = channel;
    public int WindowDays { get; set; } = windowDays;
    public string CostCenter { get; set; } = costCenter;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Rating { get; set; } = rating;
    public int BatchSize { get; set; } = batchSize;
    public LegacyScenarioSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a legacy scenario snapshot in status new.</summary>
    public static LegacyScenarioSnapshot NewLegacyScenarioSnapshot(string code, string label)
    {
        return new LegacyScenarioSnapshot(
            code,
            label,
            "",
            "",
            0,
            "",
            0,
            "",
            0,
            0.0,
            0,
            LegacyScenarioSnapshotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyScenarioSnapshotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyScenarioSnapshotStatus next)
    {
        switch (Status)
        {
            case LegacyScenarioSnapshotStatus.New:
                return next == LegacyScenarioSnapshotStatus.Running;
            case LegacyScenarioSnapshotStatus.Queued:
                switch (next)
                {
                    case LegacyScenarioSnapshotStatus.Failed:
                        return true;
                }
                return false;
            case LegacyScenarioSnapshotStatus.Running:
                switch (next)
                {
                    case LegacyScenarioSnapshotStatus.Failed:
                        return true;
                }
                return false;
            case LegacyScenarioSnapshotStatus.Done:
                switch (next)
                {
                    case LegacyScenarioSnapshotStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyScenarioSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy scenario snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"legacy scenario snapshot"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)WindowDays + (double)GraceDays, 2);
    }

    public LegacyScenarioSnapshot Copy()
    {
        return new LegacyScenarioSnapshot(
            Code,
            Label,
            Reviewer,
            Title,
            GraceDays,
            Channel,
            WindowDays,
            CostCenter,
            TenureMonths,
            Rating,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
