namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted garnishment budget is in its life cycle.</summary>
public enum WeightedGarnishmentBudgetStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a weighted garnishment budget and its current status.</summary>
public sealed class WeightedGarnishmentBudget(string code, string name, string label, double score, bool approved, int graceDays, string jobCode, string timezone, double amount, int sequence, int tenureMonths, WeightedGarnishmentBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public double Score { get; set; } = score;
    public bool Approved { get; set; } = approved;
    public int GraceDays { get; set; } = graceDays;
    public string JobCode { get; set; } = jobCode;
    public string Timezone { get; set; } = timezone;
    public double Amount { get; set; } = amount;
    public int Sequence { get; set; } = sequence;
    public int TenureMonths { get; set; } = tenureMonths;
    public WeightedGarnishmentBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a weighted garnishment budget in status requested.</summary>
    public static WeightedGarnishmentBudget NewWeightedGarnishmentBudget(string code, string name)
    {
        return new WeightedGarnishmentBudget(
            code,
            name,
            "",
            0.0,
            false,
            0,
            "",
            "",
            0.0,
            0,
            0,
            WeightedGarnishmentBudgetStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedGarnishmentBudgetStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedGarnishmentBudgetStatus next)
    {
        switch (Status)
        {
            case WeightedGarnishmentBudgetStatus.Requested:
                switch (next)
                {
                    case WeightedGarnishmentBudgetStatus.Granted:
                    case WeightedGarnishmentBudgetStatus.Expired:
                        return true;
                }
                return false;
            case WeightedGarnishmentBudgetStatus.Granted:
                switch (next)
                {
                    case WeightedGarnishmentBudgetStatus.Revoked:
                        return true;
                }
                return false;
            case WeightedGarnishmentBudgetStatus.Revoked:
                return next == WeightedGarnishmentBudgetStatus.Expired;
        }
        return false;
    }

    public void MoveTo(WeightedGarnishmentBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted garnishment budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)TenureMonths - Amount, 2);
    }

    public WeightedGarnishmentBudget Copy()
    {
        return new WeightedGarnishmentBudget(
            Code,
            Name,
            Label,
            Score,
            Approved,
            GraceDays,
            JobCode,
            Timezone,
            Amount,
            Sequence,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
