namespace Acme.Workforce.Contractors.Engagement.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy engagement budget is in its life cycle.</summary>
public enum LegacyEngagementBudgetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A legacy engagement budget as the engagement team keeps it.</summary>
public sealed class LegacyEngagementBudget(string code, string name, string location, string managerId, string country, double ratio, int sequence, double rate, LegacyEngagementBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public string ManagerId { get; set; } = managerId;
    public string Country { get; set; } = country;
    public double Ratio { get; set; } = ratio;
    public int Sequence { get; set; } = sequence;
    public double Rate { get; set; } = rate;
    public LegacyEngagementBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a legacy engagement budget in status draft.</summary>
    public static LegacyEngagementBudget NewLegacyEngagementBudget(string code, string name)
    {
        return new LegacyEngagementBudget(code, name, "", "", "", 0.0, 0, 0.0, LegacyEngagementBudgetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyEngagementBudgetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyEngagementBudgetStatus next)
    {
        switch (Status)
        {
            case LegacyEngagementBudgetStatus.Draft:
                return next == LegacyEngagementBudgetStatus.Active || next == LegacyEngagementBudgetStatus.Suspended;
            case LegacyEngagementBudgetStatus.Pending:
                switch (next)
                {
                    case LegacyEngagementBudgetStatus.Suspended:
                    case LegacyEngagementBudgetStatus.Closed:
                        return true;
                }
                return false;
            case LegacyEngagementBudgetStatus.Active:
                switch (next)
                {
                    case LegacyEngagementBudgetStatus.Suspended:
                    case LegacyEngagementBudgetStatus.Closed:
                        return true;
                }
                return false;
            case LegacyEngagementBudgetStatus.Suspended:
                return next == LegacyEngagementBudgetStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LegacyEngagementBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy engagement budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetLoad()
    {
        return Math.Round((double)Sequence + Ratio, 2);
    }

    public LegacyEngagementBudget Copy()
    {
        return new LegacyEngagementBudget(Code, Name, Location, ManagerId, Country, Ratio, Sequence, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
