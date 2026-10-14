namespace Acme.Workforce.Workplace.Occupancy.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy override is in its life cycle.</summary>
public enum OccupancyOverrideStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A occupancy override. Its code is unique within a tenant.</summary>
public sealed class OccupancyOverride(string code, string name, int revision, double ratio, bool archived, OccupancyOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public double Ratio { get; set; } = ratio;
    public bool Archived { get; set; } = archived;
    public OccupancyOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy override in status requested.</summary>
    public static OccupancyOverride NewOccupancyOverride(string code, string name)
    {
        return new OccupancyOverride(code, name, 0, 0.0, false, OccupancyOverrideStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyOverrideStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyOverrideStatus next)
    {
        switch (Status)
        {
            case OccupancyOverrideStatus.Requested:
                switch (next)
                {
                    case OccupancyOverrideStatus.Revoked:
                    case OccupancyOverrideStatus.Expired:
                        return true;
                }
                return false;
            case OccupancyOverrideStatus.Granted:
                switch (next)
                {
                    case OccupancyOverrideStatus.Revoked:
                        return true;
                }
                return false;
            case OccupancyOverrideStatus.Revoked:
                switch (next)
                {
                    case OccupancyOverrideStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Ratio + (double)Revision, 2);
    }

    public OccupancyOverride Copy()
    {
        return new OccupancyOverride(Code, Name, Revision, Ratio, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
