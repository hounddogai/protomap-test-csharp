namespace Acme.Workforce.Workplace.Occupancy.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy notice is in its life cycle.</summary>
public enum OccupancyNoticeStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a occupancy notice and its current status.</summary>
public sealed class OccupancyNotice(string code, string name, int graceDays, string department, double cost, string skill, OccupancyNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public string Department { get; set; } = department;
    public double Cost { get; set; } = cost;
    public string Skill { get; set; } = skill;
    public OccupancyNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy notice in status open.</summary>
    public static OccupancyNotice NewOccupancyNotice(string code, string name)
    {
        return new OccupancyNotice(code, name, 0, "", 0.0, "", OccupancyNoticeStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyNoticeStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyNoticeStatus next)
    {
        switch (Status)
        {
            case OccupancyNoticeStatus.Open:
                switch (next)
                {
                    case OccupancyNoticeStatus.Archived:
                        return true;
                }
                return false;
            case OccupancyNoticeStatus.InReview:
                return next == OccupancyNoticeStatus.Resolved || next == OccupancyNoticeStatus.Archived;
            case OccupancyNoticeStatus.Resolved:
                switch (next)
                {
                    case OccupancyNoticeStatus.Reopened:
                    case OccupancyNoticeStatus.Archived:
                        return true;
                }
                return false;
            case OccupancyNoticeStatus.Reopened:
                switch (next)
                {
                    case OccupancyNoticeStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)GraceDays - Cost, 2);
    }

    public OccupancyNotice Copy()
    {
        return new OccupancyNotice(Code, Name, GraceDays, Department, Cost, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
