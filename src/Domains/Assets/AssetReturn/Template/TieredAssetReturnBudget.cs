namespace Acme.Workforce.Assets.AssetReturn.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered asset return budget is in its life cycle.</summary>
public enum TieredAssetReturnBudgetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One tiered asset return budget, with what the planners need to act on it.</summary>
public sealed class TieredAssetReturnBudget(string code, string title, string department, int endsAt, int sequence, double hours, TieredAssetReturnBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Department { get; set; } = department;
    public int EndsAt { get; set; } = endsAt;
    public int Sequence { get; set; } = sequence;
    public double Hours { get; set; } = hours;
    public TieredAssetReturnBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a tiered asset return budget in status planned.</summary>
    public static TieredAssetReturnBudget NewTieredAssetReturnBudget(string code, string title)
    {
        return new TieredAssetReturnBudget(code, title, "", 0, 0, 0.0, TieredAssetReturnBudgetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredAssetReturnBudgetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredAssetReturnBudgetStatus next)
    {
        switch (Status)
        {
            case TieredAssetReturnBudgetStatus.Planned:
                return next == TieredAssetReturnBudgetStatus.InProgress;
            case TieredAssetReturnBudgetStatus.Scheduled:
                return next == TieredAssetReturnBudgetStatus.Completed || next == TieredAssetReturnBudgetStatus.Cancelled;
            case TieredAssetReturnBudgetStatus.InProgress:
                switch (next)
                {
                    case TieredAssetReturnBudgetStatus.Cancelled:
                        return true;
                }
                return false;
            case TieredAssetReturnBudgetStatus.Completed:
                return next == TieredAssetReturnBudgetStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TieredAssetReturnBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered asset return budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetLoad()
    {
        return Math.Round((double)EndsAt - Hours, 2);
    }

    public TieredAssetReturnBudget Copy()
    {
        return new TieredAssetReturnBudget(Code, Title, Department, EndsAt, Sequence, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
