namespace Acme.Workforce.Payrollops.Garnishment.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered garnishment target is in its life cycle.</summary>
public enum TieredGarnishmentTargetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One tiered garnishment target, with what the planners need to act on it.</summary>
public sealed class TieredGarnishmentTarget(string code, string label, double threshold, string jobCode, string skill, double amount, string salaryBand, double score, int quantity, double floorValue, string team, TieredGarnishmentTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Threshold { get; set; } = threshold;
    public string JobCode { get; set; } = jobCode;
    public string Skill { get; set; } = skill;
    public double Amount { get; set; } = amount;
    public string SalaryBand { get; set; } = salaryBand;
    public double Score { get; set; } = score;
    public int Quantity { get; set; } = quantity;
    public double FloorValue { get; set; } = floorValue;
    public string Team { get; set; } = team;
    public TieredGarnishmentTargetStatus Status { get; set; } = status;

    /// <summary>Starts a tiered garnishment target in status draft.</summary>
    public static TieredGarnishmentTarget NewTieredGarnishmentTarget(string code, string label)
    {
        return new TieredGarnishmentTarget(
            code,
            label,
            0.0,
            "",
            "",
            0.0,
            "",
            0.0,
            0,
            0.0,
            "",
            TieredGarnishmentTargetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredGarnishmentTargetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredGarnishmentTargetStatus next)
    {
        switch (Status)
        {
            case TieredGarnishmentTargetStatus.Draft:
                switch (next)
                {
                    case TieredGarnishmentTargetStatus.Closed:
                        return true;
                }
                return false;
            case TieredGarnishmentTargetStatus.Pending:
                return next == TieredGarnishmentTargetStatus.Active || next == TieredGarnishmentTargetStatus.Closed;
            case TieredGarnishmentTargetStatus.Active:
                switch (next)
                {
                    case TieredGarnishmentTargetStatus.Closed:
                        return true;
                }
                return false;
            case TieredGarnishmentTargetStatus.Suspended:
                return next == TieredGarnishmentTargetStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TieredGarnishmentTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered garnishment target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"tiered garnishment target"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Score * (double)Quantity, 2);
    }

    public TieredGarnishmentTarget Copy()
    {
        return new TieredGarnishmentTarget(
            Code,
            Label,
            Threshold,
            JobCode,
            Skill,
            Amount,
            SalaryBand,
            Score,
            Quantity,
            FloorValue,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
