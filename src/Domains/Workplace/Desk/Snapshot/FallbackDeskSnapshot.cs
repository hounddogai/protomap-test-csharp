namespace Acme.Workforce.Workplace.Desk.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback desk snapshot is in its life cycle.</summary>
public enum FallbackDeskSnapshotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a fallback desk snapshot and its current status.</summary>
public sealed class FallbackDeskSnapshot(string code, string title, int createdAt, string notes, double rate, string externalRef, string summary, FallbackDeskSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public string Notes { get; set; } = notes;
    public double Rate { get; set; } = rate;
    public string ExternalRef { get; set; } = externalRef;
    public string Summary { get; set; } = summary;
    public FallbackDeskSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a fallback desk snapshot in status draft.</summary>
    public static FallbackDeskSnapshot NewFallbackDeskSnapshot(string code, string title)
    {
        return new FallbackDeskSnapshot(code, title, 0, "", 0.0, "", "", FallbackDeskSnapshotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackDeskSnapshotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackDeskSnapshotStatus next)
    {
        switch (Status)
        {
            case FallbackDeskSnapshotStatus.Draft:
                switch (next)
                {
                    case FallbackDeskSnapshotStatus.Pending:
                    case FallbackDeskSnapshotStatus.Active:
                        return true;
                }
                return false;
            case FallbackDeskSnapshotStatus.Pending:
                return next == FallbackDeskSnapshotStatus.Active || next == FallbackDeskSnapshotStatus.Closed;
            case FallbackDeskSnapshotStatus.Active:
                return next == FallbackDeskSnapshotStatus.Closed;
            case FallbackDeskSnapshotStatus.Suspended:
                switch (next)
                {
                    case FallbackDeskSnapshotStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackDeskSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback desk snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"fallback desk snapshot"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)CreatedAt + Rate, 2);
    }

    public FallbackDeskSnapshot Copy()
    {
        return new FallbackDeskSnapshot(Code, Title, CreatedAt, Notes, Rate, ExternalRef, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
