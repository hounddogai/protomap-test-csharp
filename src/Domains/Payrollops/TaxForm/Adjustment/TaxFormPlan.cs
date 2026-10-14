namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form plan is in its life cycle.</summary>
public enum TaxFormPlanStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a tax form plan and its current status.</summary>
public sealed class TaxFormPlan(string code, string title, int graceDays, int leaveDays, int quantity, string skill, string salaryBand, int version, TaxFormPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int GraceDays { get; set; } = graceDays;
    public int LeaveDays { get; set; } = leaveDays;
    public int Quantity { get; set; } = quantity;
    public string Skill { get; set; } = skill;
    public string SalaryBand { get; set; } = salaryBand;
    public int Version { get; set; } = version;
    public TaxFormPlanStatus Status { get; set; } = status;

    /// <summary>Starts a tax form plan in status created.</summary>
    public static TaxFormPlan NewTaxFormPlan(string code, string title)
    {
        return new TaxFormPlan(code, title, 0, 0, 0, "", "", 0, TaxFormPlanStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormPlanStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormPlanStatus next)
    {
        switch (Status)
        {
            case TaxFormPlanStatus.Created:
                switch (next)
                {
                    case TaxFormPlanStatus.Published:
                    case TaxFormPlanStatus.Retired:
                        return true;
                }
                return false;
            case TaxFormPlanStatus.Validated:
                switch (next)
                {
                    case TaxFormPlanStatus.Published:
                    case TaxFormPlanStatus.Retired:
                        return true;
                }
                return false;
            case TaxFormPlanStatus.Published:
                return next == TaxFormPlanStatus.Retired;
        }
        return false;
    }

    public void MoveTo(TaxFormPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"tax form plan"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)LeaveDays * (double)Version, 2);
    }

    public TaxFormPlan Copy()
    {
        return new TaxFormPlan(Code, Title, GraceDays, LeaveDays, Quantity, Skill, SalaryBand, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
