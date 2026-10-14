namespace Acme.Workforce.Absence.LeaveRequest.RequestBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request allocation is in its life cycle.</summary>
public enum LeaveRequestAllocationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a leave request allocation and its current status.</summary>
public sealed class LeaveRequestAllocation(string code, string title, bool archived, string source, string location, string department, Dictionary<string, string> attributes, string reviewer, int expiresAt, double hours, LeaveRequestAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public string Source { get; set; } = source;
    public string Location { get; set; } = location;
    public string Department { get; set; } = department;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Reviewer { get; set; } = reviewer;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Hours { get; set; } = hours;
    public LeaveRequestAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a leave request allocation in status created.</summary>
    public static LeaveRequestAllocation NewLeaveRequestAllocation(string code, string title)
    {
        return new LeaveRequestAllocation(
            code,
            title,
            false,
            "",
            "",
            "",
            new Dictionary<string, string>(),
            "",
            0,
            0.0,
            LeaveRequestAllocationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestAllocationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestAllocationStatus next)
    {
        switch (Status)
        {
            case LeaveRequestAllocationStatus.Created:
                switch (next)
                {
                    case LeaveRequestAllocationStatus.Validated:
                        return true;
                }
                return false;
            case LeaveRequestAllocationStatus.Validated:
                return next == LeaveRequestAllocationStatus.Published;
            case LeaveRequestAllocationStatus.Published:
                switch (next)
                {
                    case LeaveRequestAllocationStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LeaveRequestAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetVolume()
    {
        return Math.Round((double)ExpiresAt + Hours, 2);
    }

    public LeaveRequestAllocation Copy()
    {
        return new LeaveRequestAllocation(
            Code,
            Title,
            Archived,
            Source,
            Location,
            Department,
            Attributes,
            Reviewer,
            ExpiresAt,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
