namespace Acme.Workforce.Absence.Policy.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy budget is in its life cycle.</summary>
public enum PolicyBudgetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a policy budget and its current status.</summary>
public sealed class PolicyBudget(string code, string title, string notes, string description, int updatedAt, int batchSize, string externalRef, string name, PolicyBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public string Description { get; set; } = description;
    public int UpdatedAt { get; set; } = updatedAt;
    public int BatchSize { get; set; } = batchSize;
    public string ExternalRef { get; set; } = externalRef;
    public string Name { get; set; } = name;
    public PolicyBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a policy budget in status planned.</summary>
    public static PolicyBudget NewPolicyBudget(string code, string title)
    {
        return new PolicyBudget(code, title, "", "", 0, 0, "", "", PolicyBudgetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyBudgetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyBudgetStatus next)
    {
        switch (Status)
        {
            case PolicyBudgetStatus.Planned:
                return next == PolicyBudgetStatus.Completed;
            case PolicyBudgetStatus.Scheduled:
                switch (next)
                {
                    case PolicyBudgetStatus.Completed:
                    case PolicyBudgetStatus.Cancelled:
                        return true;
                }
                return false;
            case PolicyBudgetStatus.InProgress:
                switch (next)
                {
                    case PolicyBudgetStatus.Completed:
                    case PolicyBudgetStatus.Cancelled:
                        return true;
                }
                return false;
            case PolicyBudgetStatus.Completed:
                switch (next)
                {
                    case PolicyBudgetStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"policy budget"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)UpdatedAt - (double)BatchSize, 2);
    }

    public PolicyBudget Copy()
    {
        return new PolicyBudget(Code, Title, Notes, Description, UpdatedAt, BatchSize, ExternalRef, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
