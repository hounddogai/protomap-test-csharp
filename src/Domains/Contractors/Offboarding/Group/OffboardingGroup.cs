namespace Acme.Workforce.Contractors.Offboarding.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding group is in its life cycle.</summary>
public enum OffboardingGroupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A offboarding group. Its code is unique within a tenant.</summary>
public sealed class OffboardingGroup(string code, string name, List<string> tags, int endsAt, int retries, string ownerId, int expiresAt, double rate, OffboardingGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public int EndsAt { get; set; } = endsAt;
    public int Retries { get; set; } = retries;
    public string OwnerId { get; set; } = ownerId;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Rate { get; set; } = rate;
    public OffboardingGroupStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding group in status planned.</summary>
    public static OffboardingGroup NewOffboardingGroup(string code, string name)
    {
        return new OffboardingGroup(
            code,
            name,
            new List<string>(),
            0,
            0,
            "",
            0,
            0.0,
            OffboardingGroupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingGroupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingGroupStatus next)
    {
        switch (Status)
        {
            case OffboardingGroupStatus.Planned:
                return next == OffboardingGroupStatus.Scheduled;
            case OffboardingGroupStatus.Scheduled:
                return next == OffboardingGroupStatus.InProgress || next == OffboardingGroupStatus.Cancelled;
            case OffboardingGroupStatus.InProgress:
                switch (next)
                {
                    case OffboardingGroupStatus.Completed:
                    case OffboardingGroupStatus.Cancelled:
                        return true;
                }
                return false;
            case OffboardingGroupStatus.Completed:
                return next == OffboardingGroupStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(OffboardingGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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
        return Math.Round((double)EndsAt + (double)ExpiresAt, 2);
    }

    public OffboardingGroup Copy()
    {
        return new OffboardingGroup(Code, Name, Tags, EndsAt, Retries, OwnerId, ExpiresAt, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
