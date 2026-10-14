namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return budget is in its life cycle.</summary>
public enum AssetReturnBudgetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A asset return budget. Its code is unique within a tenant.</summary>
public sealed class AssetReturnBudget(string code, string title, double ceiling, bool active, double maximum, string externalRef, string team, AssetReturnBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public bool Active { get; set; } = active;
    public double Maximum { get; set; } = maximum;
    public string ExternalRef { get; set; } = externalRef;
    public string Team { get; set; } = team;
    public AssetReturnBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a asset return budget in status draft.</summary>
    public static AssetReturnBudget NewAssetReturnBudget(string code, string title)
    {
        return new AssetReturnBudget(code, title, 0.0, false, 0.0, "", "", AssetReturnBudgetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnBudgetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnBudgetStatus next)
    {
        switch (Status)
        {
            case AssetReturnBudgetStatus.Draft:
                switch (next)
                {
                    case AssetReturnBudgetStatus.Pending:
                    case AssetReturnBudgetStatus.Active:
                        return true;
                }
                return false;
            case AssetReturnBudgetStatus.Pending:
                return next == AssetReturnBudgetStatus.Active || next == AssetReturnBudgetStatus.Suspended;
            case AssetReturnBudgetStatus.Active:
                return next == AssetReturnBudgetStatus.Closed;
            case AssetReturnBudgetStatus.Suspended:
                return next == AssetReturnBudgetStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AssetReturnBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Maximum - Ceiling, 2);
    }

    public AssetReturnBudget Copy()
    {
        return new AssetReturnBudget(Code, Title, Ceiling, Active, Maximum, ExternalRef, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
