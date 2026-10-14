namespace Acme.Workforce.Workplace.Occupancy.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy profile is in its life cycle.</summary>
public enum OccupancyProfileStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A occupancy profile. Its code is unique within a tenant.</summary>
public sealed class OccupancyProfile(string code, string title, int tenureMonths, string description, int revision, double fte, OccupancyProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Description { get; set; } = description;
    public int Revision { get; set; } = revision;
    public double Fte { get; set; } = fte;
    public OccupancyProfileStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy profile in status idle.</summary>
    public static OccupancyProfile NewOccupancyProfile(string code, string title)
    {
        return new OccupancyProfile(code, title, 0, "", 0, 0.0, OccupancyProfileStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyProfileStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyProfileStatus next)
    {
        switch (Status)
        {
            case OccupancyProfileStatus.Idle:
                return next == OccupancyProfileStatus.Warming || next == OccupancyProfileStatus.Draining;
            case OccupancyProfileStatus.Warming:
                switch (next)
                {
                    case OccupancyProfileStatus.Ready:
                    case OccupancyProfileStatus.Draining:
                        return true;
                }
                return false;
            case OccupancyProfileStatus.Ready:
                switch (next)
                {
                    case OccupancyProfileStatus.Stopped:
                        return true;
                }
                return false;
            case OccupancyProfileStatus.Draining:
                return next == OccupancyProfileStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(OccupancyProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Revision - Fte, 2);
    }

    public OccupancyProfile Copy()
    {
        return new OccupancyProfile(Code, Title, TenureMonths, Description, Revision, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
