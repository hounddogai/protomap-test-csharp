namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model notice is in its life cycle.</summary>
public enum CostModelNoticeStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A cost model notice. Its code is unique within a tenant.</summary>
public sealed class CostModelNotice(string code, string title, bool archived, string region, int updatedAt, double hours, double ceiling, string location, CostModelNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public string Region { get; set; } = region;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Hours { get; set; } = hours;
    public double Ceiling { get; set; } = ceiling;
    public string Location { get; set; } = location;
    public CostModelNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a cost model notice in status proposed.</summary>
    public static CostModelNotice NewCostModelNotice(string code, string title)
    {
        return new CostModelNotice(code, title, false, "", 0, 0.0, 0.0, "", CostModelNoticeStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelNoticeStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelNoticeStatus next)
    {
        switch (Status)
        {
            case CostModelNoticeStatus.Proposed:
                switch (next)
                {
                    case CostModelNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
            case CostModelNoticeStatus.Approved:
                switch (next)
                {
                    case CostModelNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
            case CostModelNoticeStatus.Rejected:
                switch (next)
                {
                    case CostModelNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CostModelNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"cost model notice"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Ceiling * (double)UpdatedAt, 2);
    }

    public CostModelNotice Copy()
    {
        return new CostModelNotice(Code, Title, Archived, Region, UpdatedAt, Hours, Ceiling, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
