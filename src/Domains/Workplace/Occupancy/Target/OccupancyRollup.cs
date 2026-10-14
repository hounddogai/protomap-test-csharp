namespace Acme.Workforce.Workplace.Occupancy.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy rollup is in its life cycle.</summary>
public enum OccupancyRollupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A occupancy rollup. Its code is unique within a tenant.</summary>
public sealed class OccupancyRollup(string code, string label, string checksum, string contactEmail, double score, double ratio, string description, int version, OccupancyRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public string ContactEmail { get; set; } = contactEmail;
    public double Score { get; set; } = score;
    public double Ratio { get; set; } = ratio;
    public string Description { get; set; } = description;
    public int Version { get; set; } = version;
    public OccupancyRollupStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy rollup in status open.</summary>
    public static OccupancyRollup NewOccupancyRollup(string code, string label)
    {
        return new OccupancyRollup(code, label, "", "", 0.0, 0.0, "", 0, OccupancyRollupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyRollupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyRollupStatus next)
    {
        switch (Status)
        {
            case OccupancyRollupStatus.Open:
                return next == OccupancyRollupStatus.InReview || next == OccupancyRollupStatus.Reopened;
            case OccupancyRollupStatus.InReview:
                return next == OccupancyRollupStatus.Resolved;
            case OccupancyRollupStatus.Resolved:
                switch (next)
                {
                    case OccupancyRollupStatus.Archived:
                        return true;
                }
                return false;
            case OccupancyRollupStatus.Reopened:
                switch (next)
                {
                    case OccupancyRollupStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"occupancy rollup"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Version - Ratio, 2);
    }

    public OccupancyRollup Copy()
    {
        return new OccupancyRollup(Code, Label, Checksum, ContactEmail, Score, Ratio, Description, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
