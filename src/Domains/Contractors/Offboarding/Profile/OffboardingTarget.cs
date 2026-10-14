namespace Acme.Workforce.Contractors.Offboarding.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding target is in its life cycle.</summary>
public enum OffboardingTargetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A offboarding target as the offboarding team keeps it.</summary>
public sealed class OffboardingTarget(string code, string title, int updatedAt, bool archived, bool pinned, string name, OffboardingTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Archived { get; set; } = archived;
    public bool Pinned { get; set; } = pinned;
    public string Name { get; set; } = name;
    public OffboardingTargetStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding target in status planned.</summary>
    public static OffboardingTarget NewOffboardingTarget(string code, string title)
    {
        return new OffboardingTarget(code, title, 0, false, false, "", OffboardingTargetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingTargetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingTargetStatus next)
    {
        switch (Status)
        {
            case OffboardingTargetStatus.Planned:
                return next == OffboardingTargetStatus.Cancelled;
            case OffboardingTargetStatus.Scheduled:
                return next == OffboardingTargetStatus.Cancelled;
            case OffboardingTargetStatus.InProgress:
                return next == OffboardingTargetStatus.Completed;
            case OffboardingTargetStatus.Completed:
                return next == OffboardingTargetStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(OffboardingTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"offboarding target"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)UpdatedAt;
    }

    public OffboardingTarget Copy()
    {
        return new OffboardingTarget(Code, Title, UpdatedAt, Archived, Pinned, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
