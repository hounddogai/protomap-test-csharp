namespace Acme.Workforce.Assets.Assignment.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment digest is in its life cycle.</summary>
public enum AssignmentDigestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One assignment digest, with what the planners need to act on it.</summary>
public sealed class AssignmentDigest(string code, string label, int durationMinutes, double fte, int attempts, string summary, int tenureMonths, string country, bool approved, int quantity, string contactEmail, AssignmentDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Fte { get; set; } = fte;
    public int Attempts { get; set; } = attempts;
    public string Summary { get; set; } = summary;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Country { get; set; } = country;
    public bool Approved { get; set; } = approved;
    public int Quantity { get; set; } = quantity;
    public string ContactEmail { get; set; } = contactEmail;
    public AssignmentDigestStatus Status { get; set; } = status;

    /// <summary>Starts a assignment digest in status new.</summary>
    public static AssignmentDigest NewAssignmentDigest(string code, string label)
    {
        return new AssignmentDigest(code, label, 0, 0.0, 0, "", 0, "", false, 0, "", AssignmentDigestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentDigestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentDigestStatus next)
    {
        switch (Status)
        {
            case AssignmentDigestStatus.New:
                return next == AssignmentDigestStatus.Queued || next == AssignmentDigestStatus.Done;
            case AssignmentDigestStatus.Queued:
                return next == AssignmentDigestStatus.Running || next == AssignmentDigestStatus.Failed;
            case AssignmentDigestStatus.Running:
                switch (next)
                {
                    case AssignmentDigestStatus.Failed:
                        return true;
                }
                return false;
            case AssignmentDigestStatus.Done:
                switch (next)
                {
                    case AssignmentDigestStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssignmentDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetValue()
    {
        return Math.Round((double)Attempts - (double)DurationMinutes, 2);
    }

    public AssignmentDigest Copy()
    {
        return new AssignmentDigest(
            Code,
            Label,
            DurationMinutes,
            Fte,
            Attempts,
            Summary,
            TenureMonths,
            Country,
            Approved,
            Quantity,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
