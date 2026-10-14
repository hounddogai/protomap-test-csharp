namespace Acme.Workforce.Payrollops.TaxForm.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual tax form cycle is in its life cycle.</summary>
public enum ManualTaxFormCycleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a manual tax form cycle and its current status.</summary>
public sealed class ManualTaxFormCycle(string code, string name, string locale, double score, int limit, double rate, int durationMinutes, ManualTaxFormCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public double Score { get; set; } = score;
    public int Limit { get; set; } = limit;
    public double Rate { get; set; } = rate;
    public int DurationMinutes { get; set; } = durationMinutes;
    public ManualTaxFormCycleStatus Status { get; set; } = status;

    /// <summary>Starts a manual tax form cycle in status proposed.</summary>
    public static ManualTaxFormCycle NewManualTaxFormCycle(string code, string name)
    {
        return new ManualTaxFormCycle(code, name, "", 0.0, 0, 0.0, 0, ManualTaxFormCycleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualTaxFormCycleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualTaxFormCycleStatus next)
    {
        switch (Status)
        {
            case ManualTaxFormCycleStatus.Proposed:
                return next == ManualTaxFormCycleStatus.Approved || next == ManualTaxFormCycleStatus.Rejected;
            case ManualTaxFormCycleStatus.Approved:
                return next == ManualTaxFormCycleStatus.Rejected;
            case ManualTaxFormCycleStatus.Rejected:
                return next == ManualTaxFormCycleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ManualTaxFormCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual tax form cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Limit - Rate, 2);
    }

    public ManualTaxFormCycle Copy()
    {
        return new ManualTaxFormCycle(Code, Name, Locale, Score, Limit, Rate, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
