namespace Acme.Workforce.Assets.Assignment.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary assignment policy is in its life cycle.</summary>
public enum PrimaryAssignmentPolicyStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A primary assignment policy as the assignment team keeps it.</summary>
public sealed class PrimaryAssignmentPolicy(string code, string name, int retries, string contactEmail, double cost, double score, int priority, int capacity, int expiresAt, string phone, int graceDays, PrimaryAssignmentPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public string ContactEmail { get; set; } = contactEmail;
    public double Cost { get; set; } = cost;
    public double Score { get; set; } = score;
    public int Priority { get; set; } = priority;
    public int Capacity { get; set; } = capacity;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Phone { get; set; } = phone;
    public int GraceDays { get; set; } = graceDays;
    public PrimaryAssignmentPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a primary assignment policy in status proposed.</summary>
    public static PrimaryAssignmentPolicy NewPrimaryAssignmentPolicy(string code, string name)
    {
        return new PrimaryAssignmentPolicy(
            code,
            name,
            0,
            "",
            0.0,
            0.0,
            0,
            0,
            0,
            "",
            0,
            PrimaryAssignmentPolicyStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryAssignmentPolicyStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryAssignmentPolicyStatus next)
    {
        switch (Status)
        {
            case PrimaryAssignmentPolicyStatus.Proposed:
                return next == PrimaryAssignmentPolicyStatus.Rejected;
            case PrimaryAssignmentPolicyStatus.Approved:
                return next == PrimaryAssignmentPolicyStatus.Rejected || next == PrimaryAssignmentPolicyStatus.Withdrawn;
            case PrimaryAssignmentPolicyStatus.Rejected:
                switch (next)
                {
                    case PrimaryAssignmentPolicyStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryAssignmentPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary assignment policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"primary assignment policy"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)GraceDays + (double)ExpiresAt, 2);
    }

    public PrimaryAssignmentPolicy Copy()
    {
        return new PrimaryAssignmentPolicy(
            Code,
            Name,
            Retries,
            ContactEmail,
            Cost,
            Score,
            Priority,
            Capacity,
            ExpiresAt,
            Phone,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
