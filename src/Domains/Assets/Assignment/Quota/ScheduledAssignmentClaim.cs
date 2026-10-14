namespace Acme.Workforce.Assets.Assignment.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled assignment claim is in its life cycle.</summary>
public enum ScheduledAssignmentClaimStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A scheduled assignment claim. Its code is unique within a tenant.</summary>
public sealed class ScheduledAssignmentClaim(string code, string title, double fte, string department, double hours, List<string> tags, string channel, double ceiling, bool visible, double rating, ScheduledAssignmentClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public string Department { get; set; } = department;
    public double Hours { get; set; } = hours;
    public List<string> Tags { get; set; } = tags;
    public string Channel { get; set; } = channel;
    public double Ceiling { get; set; } = ceiling;
    public bool Visible { get; set; } = visible;
    public double Rating { get; set; } = rating;
    public ScheduledAssignmentClaimStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled assignment claim in status planned.</summary>
    public static ScheduledAssignmentClaim NewScheduledAssignmentClaim(string code, string title)
    {
        return new ScheduledAssignmentClaim(
            code,
            title,
            0.0,
            "",
            0.0,
            new List<string>(),
            "",
            0.0,
            false,
            0.0,
            ScheduledAssignmentClaimStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledAssignmentClaimStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledAssignmentClaimStatus next)
    {
        switch (Status)
        {
            case ScheduledAssignmentClaimStatus.Planned:
                switch (next)
                {
                    case ScheduledAssignmentClaimStatus.InProgress:
                    case ScheduledAssignmentClaimStatus.Completed:
                        return true;
                }
                return false;
            case ScheduledAssignmentClaimStatus.Scheduled:
                return next == ScheduledAssignmentClaimStatus.InProgress || next == ScheduledAssignmentClaimStatus.Cancelled;
            case ScheduledAssignmentClaimStatus.InProgress:
                return next == ScheduledAssignmentClaimStatus.Completed;
            case ScheduledAssignmentClaimStatus.Completed:
                switch (next)
                {
                    case ScheduledAssignmentClaimStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledAssignmentClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled assignment claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled assignment claim"} {Code}";
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

    public double AdjustedVolume()
    {
        return Math.Round(Fte + Rating, 2);
    }

    public ScheduledAssignmentClaim Copy()
    {
        return new ScheduledAssignmentClaim(
            Code,
            Title,
            Fte,
            Department,
            Hours,
            Tags,
            Channel,
            Ceiling,
            Visible,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
