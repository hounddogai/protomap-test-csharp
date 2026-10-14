namespace Acme.Workforce.Contractors.Engagement.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement target is in its life cycle.</summary>
public enum EngagementTargetStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A engagement target as the engagement team keeps it.</summary>
public sealed class EngagementTarget(string code, string name, bool archived, string locale, double floorValue, EngagementTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public string Locale { get; set; } = locale;
    public double FloorValue { get; set; } = floorValue;
    public EngagementTargetStatus Status { get; set; } = status;

    /// <summary>Starts a engagement target in status requested.</summary>
    public static EngagementTarget NewEngagementTarget(string code, string name)
    {
        return new EngagementTarget(code, name, false, "", 0.0, EngagementTargetStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementTargetStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementTargetStatus next)
    {
        switch (Status)
        {
            case EngagementTargetStatus.Requested:
                switch (next)
                {
                    case EngagementTargetStatus.Granted:
                        return true;
                }
                return false;
            case EngagementTargetStatus.Granted:
                switch (next)
                {
                    case EngagementTargetStatus.Expired:
                        return true;
                }
                return false;
            case EngagementTargetStatus.Revoked:
                return next == EngagementTargetStatus.Expired;
        }
        return false;
    }

    public void MoveTo(EngagementTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"engagement target"} {Code}";
    }

    public double MeasuredValue()
    {
        return FloorValue;
    }

    public EngagementTarget Copy()
    {
        return new EngagementTarget(Code, Name, Archived, Locale, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
