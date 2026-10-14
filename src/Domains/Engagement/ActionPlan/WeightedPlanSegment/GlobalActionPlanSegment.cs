namespace Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global action plan segment is in its life cycle.</summary>
public enum GlobalActionPlanSegmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A global action plan segment. Its code is unique within a tenant.</summary>
public sealed class GlobalActionPlanSegment(string code, string name, int attempts, double ratio, string managerId, GlobalActionPlanSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public double Ratio { get; set; } = ratio;
    public string ManagerId { get; set; } = managerId;
    public GlobalActionPlanSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a global action plan segment in status requested.</summary>
    public static GlobalActionPlanSegment NewGlobalActionPlanSegment(string code, string name)
    {
        return new GlobalActionPlanSegment(code, name, 0, 0.0, "", GlobalActionPlanSegmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalActionPlanSegmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalActionPlanSegmentStatus next)
    {
        switch (Status)
        {
            case GlobalActionPlanSegmentStatus.Requested:
                return next == GlobalActionPlanSegmentStatus.Expired;
            case GlobalActionPlanSegmentStatus.Granted:
                return next == GlobalActionPlanSegmentStatus.Revoked || next == GlobalActionPlanSegmentStatus.Expired;
            case GlobalActionPlanSegmentStatus.Revoked:
                switch (next)
                {
                    case GlobalActionPlanSegmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalActionPlanSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global action plan segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Attempts - Ratio, 2);
    }

    public GlobalActionPlanSegment Copy()
    {
        return new GlobalActionPlanSegment(Code, Name, Attempts, Ratio, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
