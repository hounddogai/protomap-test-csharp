namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped license seat checkpoint is in its life cycle.</summary>
public enum CappedLicenseSeatCheckpointStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a capped license seat checkpoint and its current status.</summary>
public sealed class CappedLicenseSeatCheckpoint(string code, string title, int batchSize, int startsAt, string costCenter, CappedLicenseSeatCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public int StartsAt { get; set; } = startsAt;
    public string CostCenter { get; set; } = costCenter;
    public CappedLicenseSeatCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a capped license seat checkpoint in status new.</summary>
    public static CappedLicenseSeatCheckpoint NewCappedLicenseSeatCheckpoint(string code, string title)
    {
        return new CappedLicenseSeatCheckpoint(code, title, 0, 0, "", CappedLicenseSeatCheckpointStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedLicenseSeatCheckpointStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedLicenseSeatCheckpointStatus next)
    {
        switch (Status)
        {
            case CappedLicenseSeatCheckpointStatus.New:
                switch (next)
                {
                    case CappedLicenseSeatCheckpointStatus.Queued:
                    case CappedLicenseSeatCheckpointStatus.Failed:
                        return true;
                }
                return false;
            case CappedLicenseSeatCheckpointStatus.Queued:
                return next == CappedLicenseSeatCheckpointStatus.Done;
            case CappedLicenseSeatCheckpointStatus.Running:
                return next == CappedLicenseSeatCheckpointStatus.Done || next == CappedLicenseSeatCheckpointStatus.Failed;
            case CappedLicenseSeatCheckpointStatus.Done:
                switch (next)
                {
                    case CappedLicenseSeatCheckpointStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedLicenseSeatCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped license seat checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"capped license seat checkpoint"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)StartsAt * (double)BatchSize, 2);
    }

    public CappedLicenseSeatCheckpoint Copy()
    {
        return new CappedLicenseSeatCheckpoint(Code, Title, BatchSize, StartsAt, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
