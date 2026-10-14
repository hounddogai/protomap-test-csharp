namespace Acme.Workforce.Engagement.Feedback.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending feedback mapping is in its life cycle.</summary>
public enum PendingFeedbackMappingStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pending feedback mapping. Its code is unique within a tenant.</summary>
public sealed class PendingFeedbackMapping(string code, string name, double maximum, string title, string employeeId, int expiresAt, int limit, string contactEmail, PendingFeedbackMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Limit { get; set; } = limit;
    public string ContactEmail { get; set; } = contactEmail;
    public PendingFeedbackMappingStatus Status { get; set; } = status;

    /// <summary>Starts a pending feedback mapping in status proposed.</summary>
    public static PendingFeedbackMapping NewPendingFeedbackMapping(string code, string name)
    {
        return new PendingFeedbackMapping(code, name, 0.0, "", "", 0, 0, "", PendingFeedbackMappingStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingFeedbackMappingStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingFeedbackMappingStatus next)
    {
        switch (Status)
        {
            case PendingFeedbackMappingStatus.Proposed:
                return next == PendingFeedbackMappingStatus.Withdrawn;
            case PendingFeedbackMappingStatus.Approved:
                switch (next)
                {
                    case PendingFeedbackMappingStatus.Rejected:
                        return true;
                }
                return false;
            case PendingFeedbackMappingStatus.Rejected:
                return next == PendingFeedbackMappingStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PendingFeedbackMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending feedback mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Limit + (double)ExpiresAt, 2);
    }

    public PendingFeedbackMapping Copy()
    {
        return new PendingFeedbackMapping(
            Code,
            Name,
            Maximum,
            Title,
            EmployeeId,
            ExpiresAt,
            Limit,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
