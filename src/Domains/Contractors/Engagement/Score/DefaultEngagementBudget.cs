namespace Acme.Workforce.Contractors.Engagement.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default engagement budget is in its life cycle.</summary>
public enum DefaultEngagementBudgetStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A default engagement budget as the engagement team keeps it.</summary>
public sealed class DefaultEngagementBudget(string code, string label, int graceDays, int attempts, string phone, string title, double cost, int retries, double floorValue, string timezone, DefaultEngagementBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public int Attempts { get; set; } = attempts;
    public string Phone { get; set; } = phone;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public int Retries { get; set; } = retries;
    public double FloorValue { get; set; } = floorValue;
    public string Timezone { get; set; } = timezone;
    public DefaultEngagementBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a default engagement budget in status proposed.</summary>
    public static DefaultEngagementBudget NewDefaultEngagementBudget(string code, string label)
    {
        return new DefaultEngagementBudget(
            code,
            label,
            0,
            0,
            "",
            "",
            0.0,
            0,
            0.0,
            "",
            DefaultEngagementBudgetStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultEngagementBudgetStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultEngagementBudgetStatus next)
    {
        switch (Status)
        {
            case DefaultEngagementBudgetStatus.Proposed:
                switch (next)
                {
                    case DefaultEngagementBudgetStatus.Rejected:
                    case DefaultEngagementBudgetStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultEngagementBudgetStatus.Approved:
                switch (next)
                {
                    case DefaultEngagementBudgetStatus.Rejected:
                    case DefaultEngagementBudgetStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultEngagementBudgetStatus.Rejected:
                return next == DefaultEngagementBudgetStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DefaultEngagementBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default engagement budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)GraceDays - (double)Retries, 2);
    }

    public DefaultEngagementBudget Copy()
    {
        return new DefaultEngagementBudget(
            Code,
            Label,
            GraceDays,
            Attempts,
            Phone,
            Title,
            Cost,
            Retries,
            FloorValue,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
