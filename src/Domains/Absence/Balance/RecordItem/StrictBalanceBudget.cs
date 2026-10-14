namespace Acme.Workforce.Absence.Balance.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict balance budget is in its life cycle.</summary>
public enum StrictBalanceBudgetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A strict balance budget. Its code is unique within a tenant.</summary>
public sealed class StrictBalanceBudget(string code, string label, double fte, List<string> tags, string summary, int startsAt, int priority, string reviewer, string location, StrictBalanceBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public List<string> Tags { get; set; } = tags;
    public string Summary { get; set; } = summary;
    public int StartsAt { get; set; } = startsAt;
    public int Priority { get; set; } = priority;
    public string Reviewer { get; set; } = reviewer;
    public string Location { get; set; } = location;
    public StrictBalanceBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a strict balance budget in status planned.</summary>
    public static StrictBalanceBudget NewStrictBalanceBudget(string code, string label)
    {
        return new StrictBalanceBudget(
            code,
            label,
            0.0,
            new List<string>(),
            "",
            0,
            0,
            "",
            "",
            StrictBalanceBudgetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictBalanceBudgetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictBalanceBudgetStatus next)
    {
        switch (Status)
        {
            case StrictBalanceBudgetStatus.Planned:
                switch (next)
                {
                    case StrictBalanceBudgetStatus.Scheduled:
                        return true;
                }
                return false;
            case StrictBalanceBudgetStatus.Scheduled:
                switch (next)
                {
                    case StrictBalanceBudgetStatus.InProgress:
                    case StrictBalanceBudgetStatus.Cancelled:
                        return true;
                }
                return false;
            case StrictBalanceBudgetStatus.InProgress:
                switch (next)
                {
                    case StrictBalanceBudgetStatus.Completed:
                    case StrictBalanceBudgetStatus.Cancelled:
                        return true;
                }
                return false;
            case StrictBalanceBudgetStatus.Completed:
                return next == StrictBalanceBudgetStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(StrictBalanceBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict balance budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetTotal()
    {
        return Math.Round((double)StartsAt - Fte, 2);
    }

    public StrictBalanceBudget Copy()
    {
        return new StrictBalanceBudget(
            Code,
            Label,
            Fte,
            Tags,
            Summary,
            StartsAt,
            Priority,
            Reviewer,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
