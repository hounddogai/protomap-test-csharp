namespace Acme.Workforce.Assets.Laptop.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback laptop budget is in its life cycle.</summary>
public enum FallbackLaptopBudgetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A fallback laptop budget. Its code is unique within a tenant.</summary>
public sealed class FallbackLaptopBudget(string code, string name, string title, double rating, double amount, string externalRef, string managerId, int graceDays, FallbackLaptopBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public double Rating { get; set; } = rating;
    public double Amount { get; set; } = amount;
    public string ExternalRef { get; set; } = externalRef;
    public string ManagerId { get; set; } = managerId;
    public int GraceDays { get; set; } = graceDays;
    public FallbackLaptopBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a fallback laptop budget in status planned.</summary>
    public static FallbackLaptopBudget NewFallbackLaptopBudget(string code, string name)
    {
        return new FallbackLaptopBudget(code, name, "", 0.0, 0.0, "", "", 0, FallbackLaptopBudgetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackLaptopBudgetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackLaptopBudgetStatus next)
    {
        switch (Status)
        {
            case FallbackLaptopBudgetStatus.Planned:
                switch (next)
                {
                    case FallbackLaptopBudgetStatus.InProgress:
                        return true;
                }
                return false;
            case FallbackLaptopBudgetStatus.Scheduled:
                return next == FallbackLaptopBudgetStatus.InProgress || next == FallbackLaptopBudgetStatus.Completed;
            case FallbackLaptopBudgetStatus.InProgress:
                switch (next)
                {
                    case FallbackLaptopBudgetStatus.Completed:
                    case FallbackLaptopBudgetStatus.Cancelled:
                        return true;
                }
                return false;
            case FallbackLaptopBudgetStatus.Completed:
                switch (next)
                {
                    case FallbackLaptopBudgetStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackLaptopBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback laptop budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"fallback laptop budget"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)GraceDays + Amount, 2);
    }

    public FallbackLaptopBudget Copy()
    {
        return new FallbackLaptopBudget(
            Code,
            Name,
            Title,
            Rating,
            Amount,
            ExternalRef,
            ManagerId,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
