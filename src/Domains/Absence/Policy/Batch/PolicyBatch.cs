namespace Acme.Workforce.Absence.Policy.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy batch is in its life cycle.</summary>
public enum PolicyBatchStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A policy batch as the policy team keeps it.</summary>
public sealed class PolicyBatch(string code, string label, string externalRef, int leaveDays, double rating, int sequence, string department, double ceiling, bool pinned, string locale, PolicyBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public int LeaveDays { get; set; } = leaveDays;
    public double Rating { get; set; } = rating;
    public int Sequence { get; set; } = sequence;
    public string Department { get; set; } = department;
    public double Ceiling { get; set; } = ceiling;
    public bool Pinned { get; set; } = pinned;
    public string Locale { get; set; } = locale;
    public PolicyBatchStatus Status { get; set; } = status;

    /// <summary>Starts a policy batch in status proposed.</summary>
    public static PolicyBatch NewPolicyBatch(string code, string label)
    {
        return new PolicyBatch(code, label, "", 0, 0.0, 0, "", 0.0, false, "", PolicyBatchStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyBatchStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyBatchStatus next)
    {
        switch (Status)
        {
            case PolicyBatchStatus.Proposed:
                return next == PolicyBatchStatus.Rejected;
            case PolicyBatchStatus.Approved:
                switch (next)
                {
                    case PolicyBatchStatus.Withdrawn:
                        return true;
                }
                return false;
            case PolicyBatchStatus.Rejected:
                return next == PolicyBatchStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PolicyBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"policy batch"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Ceiling * (double)LeaveDays, 2);
    }

    public PolicyBatch Copy()
    {
        return new PolicyBatch(
            Code,
            Label,
            ExternalRef,
            LeaveDays,
            Rating,
            Sequence,
            Department,
            Ceiling,
            Pinned,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
