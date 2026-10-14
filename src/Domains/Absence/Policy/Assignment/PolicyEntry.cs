namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy entry is in its life cycle.</summary>
public enum PolicyEntryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One policy entry, with what the planners need to act on it.</summary>
public sealed class PolicyEntry(string code, string name, string salaryBand, string label, bool archived, double budget, PolicyEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public double Budget { get; set; } = budget;
    public PolicyEntryStatus Status { get; set; } = status;

    /// <summary>Starts a policy entry in status proposed.</summary>
    public static PolicyEntry NewPolicyEntry(string code, string name)
    {
        return new PolicyEntry(code, name, "", "", false, 0.0, PolicyEntryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyEntryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyEntryStatus next)
    {
        switch (Status)
        {
            case PolicyEntryStatus.Proposed:
                switch (next)
                {
                    case PolicyEntryStatus.Approved:
                    case PolicyEntryStatus.Rejected:
                        return true;
                }
                return false;
            case PolicyEntryStatus.Approved:
                return next == PolicyEntryStatus.Rejected || next == PolicyEntryStatus.Withdrawn;
            case PolicyEntryStatus.Rejected:
                return next == PolicyEntryStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PolicyEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"policy entry"} {Code}";
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public PolicyEntry Copy()
    {
        return new PolicyEntry(Code, Name, SalaryBand, Label, Archived, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
