namespace Acme.Workforce.Compliance.Certification.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive certification plan is in its life cycle.</summary>
public enum AdaptiveCertificationPlanStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One adaptive certification plan, with what the planners need to act on it.</summary>
public sealed class AdaptiveCertificationPlan(string code, string label, int revision, int createdAt, string channel, int expiresAt, int capacity, AdaptiveCertificationPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public int CreatedAt { get; set; } = createdAt;
    public string Channel { get; set; } = channel;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Capacity { get; set; } = capacity;
    public AdaptiveCertificationPlanStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive certification plan in status draft.</summary>
    public static AdaptiveCertificationPlan NewAdaptiveCertificationPlan(string code, string label)
    {
        return new AdaptiveCertificationPlan(code, label, 0, 0, "", 0, 0, AdaptiveCertificationPlanStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveCertificationPlanStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveCertificationPlanStatus next)
    {
        switch (Status)
        {
            case AdaptiveCertificationPlanStatus.Draft:
                return next == AdaptiveCertificationPlanStatus.Suspended;
            case AdaptiveCertificationPlanStatus.Pending:
                return next == AdaptiveCertificationPlanStatus.Active;
            case AdaptiveCertificationPlanStatus.Active:
                return next == AdaptiveCertificationPlanStatus.Closed;
            case AdaptiveCertificationPlanStatus.Suspended:
                return next == AdaptiveCertificationPlanStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AdaptiveCertificationPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive certification plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Revision + (double)Capacity, 2);
    }

    public AdaptiveCertificationPlan Copy()
    {
        return new AdaptiveCertificationPlan(Code, Label, Revision, CreatedAt, Channel, ExpiresAt, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
