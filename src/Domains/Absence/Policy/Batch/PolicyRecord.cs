namespace Acme.Workforce.Absence.Policy.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy record is in its life cycle.</summary>
public enum PolicyRecordStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A policy record as the policy team keeps it.</summary>
public sealed class PolicyRecord(string code, string label, double weight, bool pinned, string skill, PolicyRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Weight { get; set; } = weight;
    public bool Pinned { get; set; } = pinned;
    public string Skill { get; set; } = skill;
    public PolicyRecordStatus Status { get; set; } = status;

    /// <summary>Starts a policy record in status planned.</summary>
    public static PolicyRecord NewPolicyRecord(string code, string label)
    {
        return new PolicyRecord(code, label, 0.0, false, "", PolicyRecordStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyRecordStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyRecordStatus next)
    {
        switch (Status)
        {
            case PolicyRecordStatus.Planned:
                return next == PolicyRecordStatus.Cancelled;
            case PolicyRecordStatus.Scheduled:
                return next == PolicyRecordStatus.InProgress;
            case PolicyRecordStatus.InProgress:
                switch (next)
                {
                    case PolicyRecordStatus.Completed:
                        return true;
                }
                return false;
            case PolicyRecordStatus.Completed:
                return next == PolicyRecordStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PolicyRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return Weight;
    }

    public PolicyRecord Copy()
    {
        return new PolicyRecord(Code, Label, Weight, Pinned, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
