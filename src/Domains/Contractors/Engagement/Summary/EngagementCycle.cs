namespace Acme.Workforce.Contractors.Engagement.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement cycle is in its life cycle.</summary>
public enum EngagementCycleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A engagement cycle as the engagement team keeps it.</summary>
public sealed class EngagementCycle(string code, string label, int endsAt, string grade, int retries, int startsAt, string contactEmail, double ratio, int createdAt, double floorValue, EngagementCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int EndsAt { get; set; } = endsAt;
    public string Grade { get; set; } = grade;
    public int Retries { get; set; } = retries;
    public int StartsAt { get; set; } = startsAt;
    public string ContactEmail { get; set; } = contactEmail;
    public double Ratio { get; set; } = ratio;
    public int CreatedAt { get; set; } = createdAt;
    public double FloorValue { get; set; } = floorValue;
    public EngagementCycleStatus Status { get; set; } = status;

    /// <summary>Starts a engagement cycle in status open.</summary>
    public static EngagementCycle NewEngagementCycle(string code, string label)
    {
        return new EngagementCycle(code, label, 0, "", 0, 0, "", 0.0, 0, 0.0, EngagementCycleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementCycleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementCycleStatus next)
    {
        switch (Status)
        {
            case EngagementCycleStatus.Open:
                return next == EngagementCycleStatus.Reopened || next == EngagementCycleStatus.Archived;
            case EngagementCycleStatus.InReview:
                switch (next)
                {
                    case EngagementCycleStatus.Reopened:
                        return true;
                }
                return false;
            case EngagementCycleStatus.Resolved:
                switch (next)
                {
                    case EngagementCycleStatus.Archived:
                        return true;
                }
                return false;
            case EngagementCycleStatus.Reopened:
                switch (next)
                {
                    case EngagementCycleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round(FloorValue * (double)StartsAt, 2);
    }

    public EngagementCycle Copy()
    {
        return new EngagementCycle(
            Code,
            Label,
            EndsAt,
            Grade,
            Retries,
            StartsAt,
            ContactEmail,
            Ratio,
            CreatedAt,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
