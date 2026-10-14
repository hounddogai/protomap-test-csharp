namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict garnishment snapshot is in its life cycle.</summary>
public enum StrictGarnishmentSnapshotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a strict garnishment snapshot and its current status.</summary>
public sealed class StrictGarnishmentSnapshot(string code, string title, int retries, double amount, double threshold, double minimum, int durationMinutes, string region, string badge, double weight, StrictGarnishmentSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public double Amount { get; set; } = amount;
    public double Threshold { get; set; } = threshold;
    public double Minimum { get; set; } = minimum;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Region { get; set; } = region;
    public string Badge { get; set; } = badge;
    public double Weight { get; set; } = weight;
    public StrictGarnishmentSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a strict garnishment snapshot in status new.</summary>
    public static StrictGarnishmentSnapshot NewStrictGarnishmentSnapshot(string code, string title)
    {
        return new StrictGarnishmentSnapshot(
            code,
            title,
            0,
            0.0,
            0.0,
            0.0,
            0,
            "",
            "",
            0.0,
            StrictGarnishmentSnapshotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictGarnishmentSnapshotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictGarnishmentSnapshotStatus next)
    {
        switch (Status)
        {
            case StrictGarnishmentSnapshotStatus.New:
                switch (next)
                {
                    case StrictGarnishmentSnapshotStatus.Done:
                    case StrictGarnishmentSnapshotStatus.Failed:
                        return true;
                }
                return false;
            case StrictGarnishmentSnapshotStatus.Queued:
                return next == StrictGarnishmentSnapshotStatus.Running || next == StrictGarnishmentSnapshotStatus.Done;
            case StrictGarnishmentSnapshotStatus.Running:
                switch (next)
                {
                    case StrictGarnishmentSnapshotStatus.Failed:
                        return true;
                }
                return false;
            case StrictGarnishmentSnapshotStatus.Done:
                return next == StrictGarnishmentSnapshotStatus.Failed;
        }
        return false;
    }

    public void MoveTo(StrictGarnishmentSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict garnishment snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"strict garnishment snapshot"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Retries * (double)DurationMinutes, 2);
    }

    public StrictGarnishmentSnapshot Copy()
    {
        return new StrictGarnishmentSnapshot(
            Code,
            Title,
            Retries,
            Amount,
            Threshold,
            Minimum,
            DurationMinutes,
            Region,
            Badge,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
