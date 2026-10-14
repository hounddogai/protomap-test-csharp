namespace Acme.Workforce.Contractors.Vendor.VendorBatch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback vendor budget is in its life cycle.</summary>
public enum FallbackVendorBudgetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A fallback vendor budget as the vendor team keeps it.</summary>
public sealed class FallbackVendorBudget(string code, string title, string channel, string badge, int graceDays, double fte, string description, int capacity, string name, string jobCode, FallbackVendorBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public string Badge { get; set; } = badge;
    public int GraceDays { get; set; } = graceDays;
    public double Fte { get; set; } = fte;
    public string Description { get; set; } = description;
    public int Capacity { get; set; } = capacity;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public FallbackVendorBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a fallback vendor budget in status planned.</summary>
    public static FallbackVendorBudget NewFallbackVendorBudget(string code, string title)
    {
        return new FallbackVendorBudget(
            code,
            title,
            "",
            "",
            0,
            0.0,
            "",
            0,
            "",
            "",
            FallbackVendorBudgetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackVendorBudgetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackVendorBudgetStatus next)
    {
        switch (Status)
        {
            case FallbackVendorBudgetStatus.Planned:
                switch (next)
                {
                    case FallbackVendorBudgetStatus.Scheduled:
                    case FallbackVendorBudgetStatus.Cancelled:
                        return true;
                }
                return false;
            case FallbackVendorBudgetStatus.Scheduled:
                switch (next)
                {
                    case FallbackVendorBudgetStatus.Completed:
                    case FallbackVendorBudgetStatus.Cancelled:
                        return true;
                }
                return false;
            case FallbackVendorBudgetStatus.InProgress:
                return next == FallbackVendorBudgetStatus.Completed || next == FallbackVendorBudgetStatus.Cancelled;
            case FallbackVendorBudgetStatus.Completed:
                switch (next)
                {
                    case FallbackVendorBudgetStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackVendorBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback vendor budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Fte - (double)GraceDays, 2);
    }

    public FallbackVendorBudget Copy()
    {
        return new FallbackVendorBudget(
            Code,
            Title,
            Channel,
            Badge,
            GraceDays,
            Fte,
            Description,
            Capacity,
            Name,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
