namespace Acme.Workforce.Workplace.Occupancy.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy score is in its life cycle.</summary>
public enum OccupancyScoreStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a occupancy score and its current status.</summary>
public sealed class OccupancyScore(string code, string name, int graceDays, int sequence, string summary, bool pinned, string location, int version, string badge, OccupancyScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public int Sequence { get; set; } = sequence;
    public string Summary { get; set; } = summary;
    public bool Pinned { get; set; } = pinned;
    public string Location { get; set; } = location;
    public int Version { get; set; } = version;
    public string Badge { get; set; } = badge;
    public OccupancyScoreStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy score in status draft.</summary>
    public static OccupancyScore NewOccupancyScore(string code, string name)
    {
        return new OccupancyScore(code, name, 0, 0, "", false, "", 0, "", OccupancyScoreStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyScoreStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyScoreStatus next)
    {
        switch (Status)
        {
            case OccupancyScoreStatus.Draft:
                switch (next)
                {
                    case OccupancyScoreStatus.Pending:
                        return true;
                }
                return false;
            case OccupancyScoreStatus.Pending:
                switch (next)
                {
                    case OccupancyScoreStatus.Closed:
                        return true;
                }
                return false;
            case OccupancyScoreStatus.Active:
                return next == OccupancyScoreStatus.Closed;
            case OccupancyScoreStatus.Suspended:
                switch (next)
                {
                    case OccupancyScoreStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)GraceDays * (double)Version, 2);
    }

    public OccupancyScore Copy()
    {
        return new OccupancyScore(
            Code,
            Name,
            GraceDays,
            Sequence,
            Summary,
            Pinned,
            Location,
            Version,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
