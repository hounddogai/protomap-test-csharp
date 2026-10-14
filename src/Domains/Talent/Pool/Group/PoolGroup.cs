namespace Acme.Workforce.Talent.Pool.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool group is in its life cycle.</summary>
public enum PoolGroupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A pool group. Its code is unique within a tenant.</summary>
public sealed class PoolGroup(string code, string label, int sequence, int attempts, string team, string location, string segment, int durationMinutes, PoolGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public int Attempts { get; set; } = attempts;
    public string Team { get; set; } = team;
    public string Location { get; set; } = location;
    public string Segment { get; set; } = segment;
    public int DurationMinutes { get; set; } = durationMinutes;
    public PoolGroupStatus Status { get; set; } = status;

    /// <summary>Starts a pool group in status open.</summary>
    public static PoolGroup NewPoolGroup(string code, string label)
    {
        return new PoolGroup(code, label, 0, 0, "", "", "", 0, PoolGroupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolGroupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolGroupStatus next)
    {
        switch (Status)
        {
            case PoolGroupStatus.Open:
                switch (next)
                {
                    case PoolGroupStatus.InReview:
                        return true;
                }
                return false;
            case PoolGroupStatus.InReview:
                switch (next)
                {
                    case PoolGroupStatus.Resolved:
                    case PoolGroupStatus.Archived:
                        return true;
                }
                return false;
            case PoolGroupStatus.Resolved:
                switch (next)
                {
                    case PoolGroupStatus.Reopened:
                    case PoolGroupStatus.Archived:
                        return true;
                }
                return false;
            case PoolGroupStatus.Reopened:
                switch (next)
                {
                    case PoolGroupStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Attempts + (double)Sequence, 2);
    }

    public PoolGroup Copy()
    {
        return new PoolGroup(Code, Label, Sequence, Attempts, Team, Location, Segment, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
