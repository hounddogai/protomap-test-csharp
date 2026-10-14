namespace Acme.Workforce.Absence.LeaveRequest.RequestBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal leave request budget is in its life cycle.</summary>
public enum SeasonalLeaveRequestBudgetStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a seasonal leave request budget and its current status.</summary>
public sealed class SeasonalLeaveRequestBudget(string code, string name, int quantity, int batchSize, string costCenter, double floorValue, int windowDays, string category, bool approved, SeasonalLeaveRequestBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public int BatchSize { get; set; } = batchSize;
    public string CostCenter { get; set; } = costCenter;
    public double FloorValue { get; set; } = floorValue;
    public int WindowDays { get; set; } = windowDays;
    public string Category { get; set; } = category;
    public bool Approved { get; set; } = approved;
    public SeasonalLeaveRequestBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal leave request budget in status requested.</summary>
    public static SeasonalLeaveRequestBudget NewSeasonalLeaveRequestBudget(string code, string name)
    {
        return new SeasonalLeaveRequestBudget(
            code,
            name,
            0,
            0,
            "",
            0.0,
            0,
            "",
            false,
            SeasonalLeaveRequestBudgetStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalLeaveRequestBudgetStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalLeaveRequestBudgetStatus next)
    {
        switch (Status)
        {
            case SeasonalLeaveRequestBudgetStatus.Requested:
                return next == SeasonalLeaveRequestBudgetStatus.Granted || next == SeasonalLeaveRequestBudgetStatus.Revoked;
            case SeasonalLeaveRequestBudgetStatus.Granted:
                switch (next)
                {
                    case SeasonalLeaveRequestBudgetStatus.Revoked:
                    case SeasonalLeaveRequestBudgetStatus.Expired:
                        return true;
                }
                return false;
            case SeasonalLeaveRequestBudgetStatus.Revoked:
                return next == SeasonalLeaveRequestBudgetStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SeasonalLeaveRequestBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal leave request budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Quantity + (double)WindowDays, 2);
    }

    public SeasonalLeaveRequestBudget Copy()
    {
        return new SeasonalLeaveRequestBudget(
            Code,
            Name,
            Quantity,
            BatchSize,
            CostCenter,
            FloorValue,
            WindowDays,
            Category,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
