namespace Acme.Workforce.Compensation.EquityGrant.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant budget is in its life cycle.</summary>
public enum EquityGrantBudgetStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A equity grant budget. Its code is unique within a tenant.</summary>
public sealed class EquityGrantBudget(string code, string label, string salaryBand, string ownerId, string badge, string channel, double rate, EquityGrantBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public string OwnerId { get; set; } = ownerId;
    public string Badge { get; set; } = badge;
    public string Channel { get; set; } = channel;
    public double Rate { get; set; } = rate;
    public EquityGrantBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant budget in status proposed.</summary>
    public static EquityGrantBudget NewEquityGrantBudget(string code, string label)
    {
        return new EquityGrantBudget(code, label, "", "", "", "", 0.0, EquityGrantBudgetStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantBudgetStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantBudgetStatus next)
    {
        switch (Status)
        {
            case EquityGrantBudgetStatus.Proposed:
                switch (next)
                {
                    case EquityGrantBudgetStatus.Rejected:
                    case EquityGrantBudgetStatus.Withdrawn:
                        return true;
                }
                return false;
            case EquityGrantBudgetStatus.Approved:
                return next == EquityGrantBudgetStatus.Rejected;
            case EquityGrantBudgetStatus.Rejected:
                switch (next)
                {
                    case EquityGrantBudgetStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"equity grant budget"} {Code}";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public EquityGrantBudget Copy()
    {
        return new EquityGrantBudget(Code, Label, SalaryBand, OwnerId, Badge, Channel, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
