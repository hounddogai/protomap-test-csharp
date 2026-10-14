namespace Acme.Workforce.Assets.Refresh.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh budget is in its life cycle.</summary>
public enum RefreshBudgetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A refresh budget as the refresh team keeps it.</summary>
public sealed class RefreshBudget(string code, string name, int createdAt, string country, string team, int revision, RefreshBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public string Country { get; set; } = country;
    public string Team { get; set; } = team;
    public int Revision { get; set; } = revision;
    public RefreshBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a refresh budget in status draft.</summary>
    public static RefreshBudget NewRefreshBudget(string code, string name)
    {
        return new RefreshBudget(code, name, 0, "", "", 0, RefreshBudgetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshBudgetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshBudgetStatus next)
    {
        switch (Status)
        {
            case RefreshBudgetStatus.Draft:
                switch (next)
                {
                    case RefreshBudgetStatus.Pending:
                    case RefreshBudgetStatus.Active:
                        return true;
                }
                return false;
            case RefreshBudgetStatus.Pending:
                switch (next)
                {
                    case RefreshBudgetStatus.Suspended:
                        return true;
                }
                return false;
            case RefreshBudgetStatus.Active:
                switch (next)
                {
                    case RefreshBudgetStatus.Closed:
                        return true;
                }
                return false;
            case RefreshBudgetStatus.Suspended:
                switch (next)
                {
                    case RefreshBudgetStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"refresh budget"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)CreatedAt - (double)Revision, 2);
    }

    public RefreshBudget Copy()
    {
        return new RefreshBudget(Code, Name, CreatedAt, Country, Team, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
