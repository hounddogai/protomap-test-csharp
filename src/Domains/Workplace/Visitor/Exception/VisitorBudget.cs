namespace Acme.Workforce.Workplace.Visitor.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor budget is in its life cycle.</summary>
public enum VisitorBudgetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A visitor budget as the visitor team keeps it.</summary>
public sealed class VisitorBudget(string code, string label, string ownerId, double weight, string source, string contactEmail, string name, VisitorBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public double Weight { get; set; } = weight;
    public string Source { get; set; } = source;
    public string ContactEmail { get; set; } = contactEmail;
    public string Name { get; set; } = name;
    public VisitorBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a visitor budget in status planned.</summary>
    public static VisitorBudget NewVisitorBudget(string code, string label)
    {
        return new VisitorBudget(code, label, "", 0.0, "", "", "", VisitorBudgetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorBudgetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorBudgetStatus next)
    {
        switch (Status)
        {
            case VisitorBudgetStatus.Planned:
                switch (next)
                {
                    case VisitorBudgetStatus.Scheduled:
                        return true;
                }
                return false;
            case VisitorBudgetStatus.Scheduled:
                switch (next)
                {
                    case VisitorBudgetStatus.Cancelled:
                        return true;
                }
                return false;
            case VisitorBudgetStatus.InProgress:
                switch (next)
                {
                    case VisitorBudgetStatus.Completed:
                    case VisitorBudgetStatus.Cancelled:
                        return true;
                }
                return false;
            case VisitorBudgetStatus.Completed:
                switch (next)
                {
                    case VisitorBudgetStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"visitor budget"} {Code}";
    }

    public double MeasuredValue()
    {
        return Weight;
    }

    public VisitorBudget Copy()
    {
        return new VisitorBudget(Code, Label, OwnerId, Weight, Source, ContactEmail, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
