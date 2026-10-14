namespace Acme.Workforce.Assets.Refresh.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh milestone is in its life cycle.</summary>
public enum RefreshMilestoneStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A refresh milestone. Its code is unique within a tenant.</summary>
public sealed class RefreshMilestone(string code, string label, string category, int expiresAt, string checksum, List<string> tags, double amount, RefreshMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Checksum { get; set; } = checksum;
    public List<string> Tags { get; set; } = tags;
    public double Amount { get; set; } = amount;
    public RefreshMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a refresh milestone in status new.</summary>
    public static RefreshMilestone NewRefreshMilestone(string code, string label)
    {
        return new RefreshMilestone(code, label, "", 0, "", new List<string>(), 0.0, RefreshMilestoneStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshMilestoneStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshMilestoneStatus next)
    {
        switch (Status)
        {
            case RefreshMilestoneStatus.New:
                switch (next)
                {
                    case RefreshMilestoneStatus.Queued:
                        return true;
                }
                return false;
            case RefreshMilestoneStatus.Queued:
                switch (next)
                {
                    case RefreshMilestoneStatus.Running:
                        return true;
                }
                return false;
            case RefreshMilestoneStatus.Running:
                return next == RefreshMilestoneStatus.Failed;
            case RefreshMilestoneStatus.Done:
                switch (next)
                {
                    case RefreshMilestoneStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedTotal()
    {
        return Math.Round(Amount + (double)ExpiresAt, 2);
    }

    public RefreshMilestone Copy()
    {
        return new RefreshMilestone(Code, Label, Category, ExpiresAt, Checksum, Tags, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
