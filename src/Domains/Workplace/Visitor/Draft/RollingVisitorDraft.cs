namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling visitor draft is in its life cycle.</summary>
public enum RollingVisitorDraftStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A rolling visitor draft. Its code is unique within a tenant.</summary>
public sealed class RollingVisitorDraft(string code, string title, int priority, int updatedAt, bool pinned, string team, int capacity, string reviewer, string costCenter, string timezone, RollingVisitorDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Pinned { get; set; } = pinned;
    public string Team { get; set; } = team;
    public int Capacity { get; set; } = capacity;
    public string Reviewer { get; set; } = reviewer;
    public string CostCenter { get; set; } = costCenter;
    public string Timezone { get; set; } = timezone;
    public RollingVisitorDraftStatus Status { get; set; } = status;

    /// <summary>Starts a rolling visitor draft in status proposed.</summary>
    public static RollingVisitorDraft NewRollingVisitorDraft(string code, string title)
    {
        return new RollingVisitorDraft(
            code,
            title,
            0,
            0,
            false,
            "",
            0,
            "",
            "",
            "",
            RollingVisitorDraftStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingVisitorDraftStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingVisitorDraftStatus next)
    {
        switch (Status)
        {
            case RollingVisitorDraftStatus.Proposed:
                switch (next)
                {
                    case RollingVisitorDraftStatus.Rejected:
                    case RollingVisitorDraftStatus.Withdrawn:
                        return true;
                }
                return false;
            case RollingVisitorDraftStatus.Approved:
                return next == RollingVisitorDraftStatus.Rejected || next == RollingVisitorDraftStatus.Withdrawn;
            case RollingVisitorDraftStatus.Rejected:
                return next == RollingVisitorDraftStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RollingVisitorDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling visitor draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"rolling visitor draft"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)UpdatedAt * (double)Capacity, 2);
    }

    public RollingVisitorDraft Copy()
    {
        return new RollingVisitorDraft(
            Code,
            Title,
            Priority,
            UpdatedAt,
            Pinned,
            Team,
            Capacity,
            Reviewer,
            CostCenter,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
