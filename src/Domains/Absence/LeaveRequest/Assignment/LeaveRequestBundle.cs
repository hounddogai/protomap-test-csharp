namespace Acme.Workforce.Absence.LeaveRequest.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request bundle is in its life cycle.</summary>
public enum LeaveRequestBundleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A leave request bundle as the leave request team keeps it.</summary>
public sealed class LeaveRequestBundle(string code, string name, Dictionary<string, string> attributes, List<string> tags, int expiresAt, int windowDays, string source, string notes, LeaveRequestBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public List<string> Tags { get; set; } = tags;
    public int ExpiresAt { get; set; } = expiresAt;
    public int WindowDays { get; set; } = windowDays;
    public string Source { get; set; } = source;
    public string Notes { get; set; } = notes;
    public LeaveRequestBundleStatus Status { get; set; } = status;

    /// <summary>Starts a leave request bundle in status idle.</summary>
    public static LeaveRequestBundle NewLeaveRequestBundle(string code, string name)
    {
        return new LeaveRequestBundle(
            code,
            name,
            new Dictionary<string, string>(),
            new List<string>(),
            0,
            0,
            "",
            "",
            LeaveRequestBundleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestBundleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestBundleStatus next)
    {
        switch (Status)
        {
            case LeaveRequestBundleStatus.Idle:
                return next == LeaveRequestBundleStatus.Ready;
            case LeaveRequestBundleStatus.Warming:
                return next == LeaveRequestBundleStatus.Draining || next == LeaveRequestBundleStatus.Stopped;
            case LeaveRequestBundleStatus.Ready:
                return next == LeaveRequestBundleStatus.Draining || next == LeaveRequestBundleStatus.Stopped;
            case LeaveRequestBundleStatus.Draining:
                switch (next)
                {
                    case LeaveRequestBundleStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LeaveRequestBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double WeightedValue()
    {
        return Math.Round((double)WindowDays * (double)ExpiresAt, 2);
    }

    public LeaveRequestBundle Copy()
    {
        return new LeaveRequestBundle(Code, Name, Attributes, Tags, ExpiresAt, WindowDays, Source, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
