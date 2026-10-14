namespace Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global action plan entry is in its life cycle.</summary>
public enum GlobalActionPlanEntryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A global action plan entry as the action plan team keeps it.</summary>
public sealed class GlobalActionPlanEntry(string code, string title, string salaryBand, string costCenter, int attempts, int createdAt, double weight, string currency, double budget, double discount, string badge, GlobalActionPlanEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public string CostCenter { get; set; } = costCenter;
    public int Attempts { get; set; } = attempts;
    public int CreatedAt { get; set; } = createdAt;
    public double Weight { get; set; } = weight;
    public string Currency { get; set; } = currency;
    public double Budget { get; set; } = budget;
    public double Discount { get; set; } = discount;
    public string Badge { get; set; } = badge;
    public GlobalActionPlanEntryStatus Status { get; set; } = status;

    /// <summary>Starts a global action plan entry in status draft.</summary>
    public static GlobalActionPlanEntry NewGlobalActionPlanEntry(string code, string title)
    {
        return new GlobalActionPlanEntry(
            code,
            title,
            "",
            "",
            0,
            0,
            0.0,
            "",
            0.0,
            0.0,
            "",
            GlobalActionPlanEntryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalActionPlanEntryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalActionPlanEntryStatus next)
    {
        switch (Status)
        {
            case GlobalActionPlanEntryStatus.Draft:
                return next == GlobalActionPlanEntryStatus.Active || next == GlobalActionPlanEntryStatus.Suspended;
            case GlobalActionPlanEntryStatus.Pending:
                return next == GlobalActionPlanEntryStatus.Suspended || next == GlobalActionPlanEntryStatus.Closed;
            case GlobalActionPlanEntryStatus.Active:
                return next == GlobalActionPlanEntryStatus.Suspended || next == GlobalActionPlanEntryStatus.Closed;
            case GlobalActionPlanEntryStatus.Suspended:
                switch (next)
                {
                    case GlobalActionPlanEntryStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalActionPlanEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global action plan entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Weight + (double)Attempts, 2);
    }

    public GlobalActionPlanEntry Copy()
    {
        return new GlobalActionPlanEntry(
            Code,
            Title,
            SalaryBand,
            CostCenter,
            Attempts,
            CreatedAt,
            Weight,
            Currency,
            Budget,
            Discount,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
