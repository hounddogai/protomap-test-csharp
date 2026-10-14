namespace Acme.Workforce.Orgdesign.Position.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position budget is in its life cycle.</summary>
public enum PositionBudgetStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A position budget as the position team keeps it.</summary>
public sealed class PositionBudget(string code, string name, string timezone, int limit, string reviewer, double rating, int version, bool approved, double ratio, double margin, PositionBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Timezone { get; set; } = timezone;
    public int Limit { get; set; } = limit;
    public string Reviewer { get; set; } = reviewer;
    public double Rating { get; set; } = rating;
    public int Version { get; set; } = version;
    public bool Approved { get; set; } = approved;
    public double Ratio { get; set; } = ratio;
    public double Margin { get; set; } = margin;
    public PositionBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a position budget in status proposed.</summary>
    public static PositionBudget NewPositionBudget(string code, string name)
    {
        return new PositionBudget(code, name, "", 0, "", 0.0, 0, false, 0.0, 0.0, PositionBudgetStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionBudgetStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionBudgetStatus next)
    {
        switch (Status)
        {
            case PositionBudgetStatus.Proposed:
                return next == PositionBudgetStatus.Approved;
            case PositionBudgetStatus.Approved:
                switch (next)
                {
                    case PositionBudgetStatus.Withdrawn:
                        return true;
                }
                return false;
            case PositionBudgetStatus.Rejected:
                return next == PositionBudgetStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PositionBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Version * Ratio, 2);
    }

    public PositionBudget Copy()
    {
        return new PositionBudget(
            Code,
            Name,
            Timezone,
            Limit,
            Reviewer,
            Rating,
            Version,
            Approved,
            Ratio,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
