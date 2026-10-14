namespace Acme.Workforce.Assets.Refresh.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending refresh mapping is in its life cycle.</summary>
public enum PendingRefreshMappingStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pending refresh mapping as the refresh team keeps it.</summary>
public sealed class PendingRefreshMapping(string code, string label, List<string> tags, int capacity, string reviewer, int limit, int startsAt, PendingRefreshMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public List<string> Tags { get; set; } = tags;
    public int Capacity { get; set; } = capacity;
    public string Reviewer { get; set; } = reviewer;
    public int Limit { get; set; } = limit;
    public int StartsAt { get; set; } = startsAt;
    public PendingRefreshMappingStatus Status { get; set; } = status;

    /// <summary>Starts a pending refresh mapping in status planned.</summary>
    public static PendingRefreshMapping NewPendingRefreshMapping(string code, string label)
    {
        return new PendingRefreshMapping(
            code,
            label,
            new List<string>(),
            0,
            "",
            0,
            0,
            PendingRefreshMappingStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingRefreshMappingStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingRefreshMappingStatus next)
    {
        switch (Status)
        {
            case PendingRefreshMappingStatus.Planned:
                switch (next)
                {
                    case PendingRefreshMappingStatus.InProgress:
                        return true;
                }
                return false;
            case PendingRefreshMappingStatus.Scheduled:
                switch (next)
                {
                    case PendingRefreshMappingStatus.InProgress:
                    case PendingRefreshMappingStatus.Cancelled:
                        return true;
                }
                return false;
            case PendingRefreshMappingStatus.InProgress:
                return next == PendingRefreshMappingStatus.Completed || next == PendingRefreshMappingStatus.Cancelled;
            case PendingRefreshMappingStatus.Completed:
                return next == PendingRefreshMappingStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PendingRefreshMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending refresh mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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

    public double ProjectedVolume()
    {
        return Math.Round((double)Capacity - (double)StartsAt, 2);
    }

    public PendingRefreshMapping Copy()
    {
        return new PendingRefreshMapping(Code, Label, Tags, Capacity, Reviewer, Limit, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
