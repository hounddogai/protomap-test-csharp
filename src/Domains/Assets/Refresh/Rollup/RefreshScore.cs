namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh score is in its life cycle.</summary>
public enum RefreshScoreStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A refresh score. Its code is unique within a tenant.</summary>
public sealed class RefreshScore(string code, string name, double budget, string category, string department, string employeeId, double amount, double ratio, double cost, int retries, RefreshScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public string Category { get; set; } = category;
    public string Department { get; set; } = department;
    public string EmployeeId { get; set; } = employeeId;
    public double Amount { get; set; } = amount;
    public double Ratio { get; set; } = ratio;
    public double Cost { get; set; } = cost;
    public int Retries { get; set; } = retries;
    public RefreshScoreStatus Status { get; set; } = status;

    /// <summary>Starts a refresh score in status planned.</summary>
    public static RefreshScore NewRefreshScore(string code, string name)
    {
        return new RefreshScore(code, name, 0.0, "", "", "", 0.0, 0.0, 0.0, 0, RefreshScoreStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshScoreStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshScoreStatus next)
    {
        switch (Status)
        {
            case RefreshScoreStatus.Planned:
                return next == RefreshScoreStatus.InProgress || next == RefreshScoreStatus.Completed;
            case RefreshScoreStatus.Scheduled:
                switch (next)
                {
                    case RefreshScoreStatus.Cancelled:
                        return true;
                }
                return false;
            case RefreshScoreStatus.InProgress:
                switch (next)
                {
                    case RefreshScoreStatus.Completed:
                    case RefreshScoreStatus.Cancelled:
                        return true;
                }
                return false;
            case RefreshScoreStatus.Completed:
                switch (next)
                {
                    case RefreshScoreStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"refresh score"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Ratio - Budget, 2);
    }

    public RefreshScore Copy()
    {
        return new RefreshScore(
            Code,
            Name,
            Budget,
            Category,
            Department,
            EmployeeId,
            Amount,
            Ratio,
            Cost,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
