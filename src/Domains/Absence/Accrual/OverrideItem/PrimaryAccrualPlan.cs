namespace Acme.Workforce.Absence.Accrual.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary accrual plan is in its life cycle.</summary>
public enum PrimaryAccrualPlanStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A primary accrual plan as the accrual team keeps it.</summary>
public sealed class PrimaryAccrualPlan(string code, string label, double score, double fte, string country, string checksum, PrimaryAccrualPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Score { get; set; } = score;
    public double Fte { get; set; } = fte;
    public string Country { get; set; } = country;
    public string Checksum { get; set; } = checksum;
    public PrimaryAccrualPlanStatus Status { get; set; } = status;

    /// <summary>Starts a primary accrual plan in status created.</summary>
    public static PrimaryAccrualPlan NewPrimaryAccrualPlan(string code, string label)
    {
        return new PrimaryAccrualPlan(code, label, 0.0, 0.0, "", "", PrimaryAccrualPlanStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryAccrualPlanStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryAccrualPlanStatus next)
    {
        switch (Status)
        {
            case PrimaryAccrualPlanStatus.Created:
                return next == PrimaryAccrualPlanStatus.Retired;
            case PrimaryAccrualPlanStatus.Validated:
                return next == PrimaryAccrualPlanStatus.Published;
            case PrimaryAccrualPlanStatus.Published:
                switch (next)
                {
                    case PrimaryAccrualPlanStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryAccrualPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary accrual plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"primary accrual plan"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Fte * Score, 2);
    }

    public PrimaryAccrualPlan Copy()
    {
        return new PrimaryAccrualPlan(Code, Label, Score, Fte, Country, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
