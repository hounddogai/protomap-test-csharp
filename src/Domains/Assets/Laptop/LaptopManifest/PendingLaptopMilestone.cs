namespace Acme.Workforce.Assets.Laptop.LaptopManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending laptop milestone is in its life cycle.</summary>
public enum PendingLaptopMilestoneStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One pending laptop milestone, with what the planners need to act on it.</summary>
public sealed class PendingLaptopMilestone(string code, string title, string name, string label, bool pinned, string managerId, int expiresAt, int windowDays, PendingLaptopMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public string ManagerId { get; set; } = managerId;
    public int ExpiresAt { get; set; } = expiresAt;
    public int WindowDays { get; set; } = windowDays;
    public PendingLaptopMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a pending laptop milestone in status open.</summary>
    public static PendingLaptopMilestone NewPendingLaptopMilestone(string code, string title)
    {
        return new PendingLaptopMilestone(code, title, "", "", false, "", 0, 0, PendingLaptopMilestoneStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingLaptopMilestoneStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingLaptopMilestoneStatus next)
    {
        switch (Status)
        {
            case PendingLaptopMilestoneStatus.Open:
                switch (next)
                {
                    case PendingLaptopMilestoneStatus.Reopened:
                    case PendingLaptopMilestoneStatus.Archived:
                        return true;
                }
                return false;
            case PendingLaptopMilestoneStatus.InReview:
                switch (next)
                {
                    case PendingLaptopMilestoneStatus.Reopened:
                    case PendingLaptopMilestoneStatus.Archived:
                        return true;
                }
                return false;
            case PendingLaptopMilestoneStatus.Resolved:
                switch (next)
                {
                    case PendingLaptopMilestoneStatus.Reopened:
                    case PendingLaptopMilestoneStatus.Archived:
                        return true;
                }
                return false;
            case PendingLaptopMilestoneStatus.Reopened:
                switch (next)
                {
                    case PendingLaptopMilestoneStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingLaptopMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending laptop milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"pending laptop milestone"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)ExpiresAt + (double)WindowDays, 2);
    }

    public PendingLaptopMilestone Copy()
    {
        return new PendingLaptopMilestone(
            Code,
            Title,
            Name,
            Label,
            Pinned,
            ManagerId,
            ExpiresAt,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
