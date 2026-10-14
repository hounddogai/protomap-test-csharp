namespace Acme.Workforce.Workplace.Desk.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict desk budget is in its life cycle.</summary>
public enum StrictDeskBudgetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A strict desk budget. Its code is unique within a tenant.</summary>
public sealed class StrictDeskBudget(string code, string label, double maximum, List<string> tags, double ceiling, double fte, string unit, StrictDeskBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public List<string> Tags { get; set; } = tags;
    public double Ceiling { get; set; } = ceiling;
    public double Fte { get; set; } = fte;
    public string Unit { get; set; } = unit;
    public StrictDeskBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a strict desk budget in status draft.</summary>
    public static StrictDeskBudget NewStrictDeskBudget(string code, string label)
    {
        return new StrictDeskBudget(
            code,
            label,
            0.0,
            new List<string>(),
            0.0,
            0.0,
            "",
            StrictDeskBudgetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictDeskBudgetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictDeskBudgetStatus next)
    {
        switch (Status)
        {
            case StrictDeskBudgetStatus.Draft:
                switch (next)
                {
                    case StrictDeskBudgetStatus.Active:
                        return true;
                }
                return false;
            case StrictDeskBudgetStatus.Pending:
                switch (next)
                {
                    case StrictDeskBudgetStatus.Active:
                    case StrictDeskBudgetStatus.Suspended:
                        return true;
                }
                return false;
            case StrictDeskBudgetStatus.Active:
                return next == StrictDeskBudgetStatus.Suspended || next == StrictDeskBudgetStatus.Closed;
            case StrictDeskBudgetStatus.Suspended:
                switch (next)
                {
                    case StrictDeskBudgetStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictDeskBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict desk budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
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

    public double EffectiveExposure()
    {
        return Math.Round(Fte + Ceiling, 2);
    }

    public StrictDeskBudget Copy()
    {
        return new StrictDeskBudget(Code, Label, Maximum, Tags, Ceiling, Fte, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
