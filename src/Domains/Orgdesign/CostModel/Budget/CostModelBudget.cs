namespace Acme.Workforce.Orgdesign.CostModel.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model budget is in its life cycle.</summary>
public enum CostModelBudgetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a cost model budget and its current status.</summary>
public sealed class CostModelBudget(string code, string title, int endsAt, string country, string ownerId, double budget, int expiresAt, string managerId, CostModelBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public string Country { get; set; } = country;
    public string OwnerId { get; set; } = ownerId;
    public double Budget { get; set; } = budget;
    public int ExpiresAt { get; set; } = expiresAt;
    public string ManagerId { get; set; } = managerId;
    public CostModelBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a cost model budget in status idle.</summary>
    public static CostModelBudget NewCostModelBudget(string code, string title)
    {
        return new CostModelBudget(code, title, 0, "", "", 0.0, 0, "", CostModelBudgetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelBudgetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelBudgetStatus next)
    {
        switch (Status)
        {
            case CostModelBudgetStatus.Idle:
                switch (next)
                {
                    case CostModelBudgetStatus.Stopped:
                        return true;
                }
                return false;
            case CostModelBudgetStatus.Warming:
                switch (next)
                {
                    case CostModelBudgetStatus.Ready:
                        return true;
                }
                return false;
            case CostModelBudgetStatus.Ready:
                return next == CostModelBudgetStatus.Draining;
            case CostModelBudgetStatus.Draining:
                switch (next)
                {
                    case CostModelBudgetStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CostModelBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)EndsAt - Budget, 2);
    }

    public CostModelBudget Copy()
    {
        return new CostModelBudget(Code, Title, EndsAt, Country, OwnerId, Budget, ExpiresAt, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
