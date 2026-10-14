namespace Acme.Workforce.Orgdesign.Position.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position manifest is in its life cycle.</summary>
public enum PositionManifestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a position manifest and its current status.</summary>
public sealed class PositionManifest(string code, string title, int durationMinutes, string managerId, string notes, int graceDays, string source, int windowDays, string reviewer, bool approved, PositionManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string ManagerId { get; set; } = managerId;
    public string Notes { get; set; } = notes;
    public int GraceDays { get; set; } = graceDays;
    public string Source { get; set; } = source;
    public int WindowDays { get; set; } = windowDays;
    public string Reviewer { get; set; } = reviewer;
    public bool Approved { get; set; } = approved;
    public PositionManifestStatus Status { get; set; } = status;

    /// <summary>Starts a position manifest in status open.</summary>
    public static PositionManifest NewPositionManifest(string code, string title)
    {
        return new PositionManifest(code, title, 0, "", "", 0, "", 0, "", false, PositionManifestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionManifestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionManifestStatus next)
    {
        switch (Status)
        {
            case PositionManifestStatus.Open:
                return next == PositionManifestStatus.Resolved;
            case PositionManifestStatus.InReview:
                return next == PositionManifestStatus.Resolved;
            case PositionManifestStatus.Resolved:
                switch (next)
                {
                    case PositionManifestStatus.Reopened:
                    case PositionManifestStatus.Archived:
                        return true;
                }
                return false;
            case PositionManifestStatus.Reopened:
                switch (next)
                {
                    case PositionManifestStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PositionManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetValue()
    {
        return Math.Round((double)WindowDays * (double)DurationMinutes, 2);
    }

    public PositionManifest Copy()
    {
        return new PositionManifest(
            Code,
            Title,
            DurationMinutes,
            ManagerId,
            Notes,
            GraceDays,
            Source,
            WindowDays,
            Reviewer,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
