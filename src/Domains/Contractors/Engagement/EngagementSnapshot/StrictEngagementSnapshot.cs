namespace Acme.Workforce.Contractors.Engagement.EngagementSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict engagement snapshot is in its life cycle.</summary>
public enum StrictEngagementSnapshotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A strict engagement snapshot as the engagement team keeps it.</summary>
public sealed class StrictEngagementSnapshot(string code, string title, string notes, string currency, string country, int batchSize, double score, StrictEngagementSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public string Currency { get; set; } = currency;
    public string Country { get; set; } = country;
    public int BatchSize { get; set; } = batchSize;
    public double Score { get; set; } = score;
    public StrictEngagementSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a strict engagement snapshot in status created.</summary>
    public static StrictEngagementSnapshot NewStrictEngagementSnapshot(string code, string title)
    {
        return new StrictEngagementSnapshot(code, title, "", "", "", 0, 0.0, StrictEngagementSnapshotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictEngagementSnapshotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictEngagementSnapshotStatus next)
    {
        switch (Status)
        {
            case StrictEngagementSnapshotStatus.Created:
                switch (next)
                {
                    case StrictEngagementSnapshotStatus.Published:
                    case StrictEngagementSnapshotStatus.Retired:
                        return true;
                }
                return false;
            case StrictEngagementSnapshotStatus.Validated:
                return next == StrictEngagementSnapshotStatus.Published;
            case StrictEngagementSnapshotStatus.Published:
                switch (next)
                {
                    case StrictEngagementSnapshotStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictEngagementSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict engagement snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"strict engagement snapshot"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)BatchSize - Score, 2);
    }

    public StrictEngagementSnapshot Copy()
    {
        return new StrictEngagementSnapshot(Code, Title, Notes, Currency, Country, BatchSize, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
