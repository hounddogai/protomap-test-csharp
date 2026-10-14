namespace Acme.Workforce.Orgdesign.CostModel.ModelPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled cost model policy is in its life cycle.</summary>
public enum ScheduledCostModelPolicyStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A scheduled cost model policy as the cost model team keeps it.</summary>
public sealed class ScheduledCostModelPolicy(string code, string title, int updatedAt, bool active, int priority, bool approved, double discount, string locale, string category, ScheduledCostModelPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Active { get; set; } = active;
    public int Priority { get; set; } = priority;
    public bool Approved { get; set; } = approved;
    public double Discount { get; set; } = discount;
    public string Locale { get; set; } = locale;
    public string Category { get; set; } = category;
    public ScheduledCostModelPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled cost model policy in status created.</summary>
    public static ScheduledCostModelPolicy NewScheduledCostModelPolicy(string code, string title)
    {
        return new ScheduledCostModelPolicy(
            code,
            title,
            0,
            false,
            0,
            false,
            0.0,
            "",
            "",
            ScheduledCostModelPolicyStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledCostModelPolicyStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledCostModelPolicyStatus next)
    {
        switch (Status)
        {
            case ScheduledCostModelPolicyStatus.Created:
                switch (next)
                {
                    case ScheduledCostModelPolicyStatus.Validated:
                        return true;
                }
                return false;
            case ScheduledCostModelPolicyStatus.Validated:
                switch (next)
                {
                    case ScheduledCostModelPolicyStatus.Published:
                    case ScheduledCostModelPolicyStatus.Retired:
                        return true;
                }
                return false;
            case ScheduledCostModelPolicyStatus.Published:
                return next == ScheduledCostModelPolicyStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ScheduledCostModelPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled cost model policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)UpdatedAt * (double)Priority, 2);
    }

    public ScheduledCostModelPolicy Copy()
    {
        return new ScheduledCostModelPolicy(
            Code,
            Title,
            UpdatedAt,
            Active,
            Priority,
            Approved,
            Discount,
            Locale,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
