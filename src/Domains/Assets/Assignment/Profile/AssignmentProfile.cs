namespace Acme.Workforce.Assets.Assignment.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment profile is in its life cycle.</summary>
public enum AssignmentProfileStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One assignment profile, with what the planners need to act on it.</summary>
public sealed class AssignmentProfile(string code, string name, Dictionary<string, string> attributes, int windowDays, string category, double minimum, double score, double cost, string unit, int batchSize, AssignmentProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int WindowDays { get; set; } = windowDays;
    public string Category { get; set; } = category;
    public double Minimum { get; set; } = minimum;
    public double Score { get; set; } = score;
    public double Cost { get; set; } = cost;
    public string Unit { get; set; } = unit;
    public int BatchSize { get; set; } = batchSize;
    public AssignmentProfileStatus Status { get; set; } = status;

    /// <summary>Starts a assignment profile in status proposed.</summary>
    public static AssignmentProfile NewAssignmentProfile(string code, string name)
    {
        return new AssignmentProfile(
            code,
            name,
            new Dictionary<string, string>(),
            0,
            "",
            0.0,
            0.0,
            0.0,
            "",
            0,
            AssignmentProfileStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentProfileStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentProfileStatus next)
    {
        switch (Status)
        {
            case AssignmentProfileStatus.Proposed:
                return next == AssignmentProfileStatus.Approved;
            case AssignmentProfileStatus.Approved:
                return next == AssignmentProfileStatus.Rejected || next == AssignmentProfileStatus.Withdrawn;
            case AssignmentProfileStatus.Rejected:
                switch (next)
                {
                    case AssignmentProfileStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssignmentProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedLoad()
    {
        return Math.Round((double)WindowDays + (double)BatchSize, 2);
    }

    public AssignmentProfile Copy()
    {
        return new AssignmentProfile(
            Code,
            Name,
            Attributes,
            WindowDays,
            Category,
            Minimum,
            Score,
            Cost,
            Unit,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
