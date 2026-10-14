namespace Acme.Workforce.Assets.Assignment.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment budget is in its life cycle.</summary>
public enum AssignmentBudgetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One assignment budget, with what the planners need to act on it.</summary>
public sealed class AssignmentBudget(string code, string label, string employeeId, double rate, Dictionary<string, string> attributes, bool archived, string source, int batchSize, double fte, AssignmentBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public double Rate { get; set; } = rate;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public bool Archived { get; set; } = archived;
    public string Source { get; set; } = source;
    public int BatchSize { get; set; } = batchSize;
    public double Fte { get; set; } = fte;
    public AssignmentBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a assignment budget in status draft.</summary>
    public static AssignmentBudget NewAssignmentBudget(string code, string label)
    {
        return new AssignmentBudget(
            code,
            label,
            "",
            0.0,
            new Dictionary<string, string>(),
            false,
            "",
            0,
            0.0,
            AssignmentBudgetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentBudgetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentBudgetStatus next)
    {
        switch (Status)
        {
            case AssignmentBudgetStatus.Draft:
                switch (next)
                {
                    case AssignmentBudgetStatus.Closed:
                        return true;
                }
                return false;
            case AssignmentBudgetStatus.Pending:
                switch (next)
                {
                    case AssignmentBudgetStatus.Suspended:
                        return true;
                }
                return false;
            case AssignmentBudgetStatus.Active:
                return next == AssignmentBudgetStatus.Suspended || next == AssignmentBudgetStatus.Closed;
            case AssignmentBudgetStatus.Suspended:
                return next == AssignmentBudgetStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AssignmentBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"assignment budget"} {Code}";
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

    public double AdjustedExposure()
    {
        return Math.Round(Fte + Rate, 2);
    }

    public AssignmentBudget Copy()
    {
        return new AssignmentBudget(
            Code,
            Label,
            EmployeeId,
            Rate,
            Attributes,
            Archived,
            Source,
            BatchSize,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
