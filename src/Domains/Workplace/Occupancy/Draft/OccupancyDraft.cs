namespace Acme.Workforce.Workplace.Occupancy.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy draft is in its life cycle.</summary>
public enum OccupancyDraftStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a occupancy draft and its current status.</summary>
public sealed class OccupancyDraft(string code, string label, double score, string name, string summary, string managerId, int leaveDays, double weight, string source, double budget, OccupancyDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Score { get; set; } = score;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public string ManagerId { get; set; } = managerId;
    public int LeaveDays { get; set; } = leaveDays;
    public double Weight { get; set; } = weight;
    public string Source { get; set; } = source;
    public double Budget { get; set; } = budget;
    public OccupancyDraftStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy draft in status proposed.</summary>
    public static OccupancyDraft NewOccupancyDraft(string code, string label)
    {
        return new OccupancyDraft(code, label, 0.0, "", "", "", 0, 0.0, "", 0.0, OccupancyDraftStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyDraftStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyDraftStatus next)
    {
        switch (Status)
        {
            case OccupancyDraftStatus.Proposed:
                return next == OccupancyDraftStatus.Rejected;
            case OccupancyDraftStatus.Approved:
                return next == OccupancyDraftStatus.Rejected;
            case OccupancyDraftStatus.Rejected:
                switch (next)
                {
                    case OccupancyDraftStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Score + Weight, 2);
    }

    public OccupancyDraft Copy()
    {
        return new OccupancyDraft(
            Code,
            Label,
            Score,
            Name,
            Summary,
            ManagerId,
            LeaveDays,
            Weight,
            Source,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
