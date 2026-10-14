namespace Acme.Workforce.Engagement.ActionPlan.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary action plan mapping is in its life cycle.</summary>
public enum PrimaryActionPlanMappingStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a primary action plan mapping and its current status.</summary>
public sealed class PrimaryActionPlanMapping(string code, string title, double fte, string segment, int revision, string ownerId, PrimaryActionPlanMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public string Segment { get; set; } = segment;
    public int Revision { get; set; } = revision;
    public string OwnerId { get; set; } = ownerId;
    public PrimaryActionPlanMappingStatus Status { get; set; } = status;

    /// <summary>Starts a primary action plan mapping in status proposed.</summary>
    public static PrimaryActionPlanMapping NewPrimaryActionPlanMapping(string code, string title)
    {
        return new PrimaryActionPlanMapping(code, title, 0.0, "", 0, "", PrimaryActionPlanMappingStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryActionPlanMappingStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryActionPlanMappingStatus next)
    {
        switch (Status)
        {
            case PrimaryActionPlanMappingStatus.Proposed:
                return next == PrimaryActionPlanMappingStatus.Rejected;
            case PrimaryActionPlanMappingStatus.Approved:
                switch (next)
                {
                    case PrimaryActionPlanMappingStatus.Rejected:
                        return true;
                }
                return false;
            case PrimaryActionPlanMappingStatus.Rejected:
                return next == PrimaryActionPlanMappingStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PrimaryActionPlanMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary action plan mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"primary action plan mapping"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Fte - (double)Revision, 2);
    }

    public PrimaryActionPlanMapping Copy()
    {
        return new PrimaryActionPlanMapping(Code, Title, Fte, Segment, Revision, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
