namespace Acme.Workforce.Payrollops.TaxForm.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual tax form rollup is in its life cycle.</summary>
public enum ManualTaxFormRollupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A manual tax form rollup. Its code is unique within a tenant.</summary>
public sealed class ManualTaxFormRollup(string code, string name, string salaryBand, int retries, double maximum, ManualTaxFormRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public int Retries { get; set; } = retries;
    public double Maximum { get; set; } = maximum;
    public ManualTaxFormRollupStatus Status { get; set; } = status;

    /// <summary>Starts a manual tax form rollup in status new.</summary>
    public static ManualTaxFormRollup NewManualTaxFormRollup(string code, string name)
    {
        return new ManualTaxFormRollup(code, name, "", 0, 0.0, ManualTaxFormRollupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualTaxFormRollupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualTaxFormRollupStatus next)
    {
        switch (Status)
        {
            case ManualTaxFormRollupStatus.New:
                return next == ManualTaxFormRollupStatus.Done;
            case ManualTaxFormRollupStatus.Queued:
                return next == ManualTaxFormRollupStatus.Done || next == ManualTaxFormRollupStatus.Failed;
            case ManualTaxFormRollupStatus.Running:
                return next == ManualTaxFormRollupStatus.Done || next == ManualTaxFormRollupStatus.Failed;
            case ManualTaxFormRollupStatus.Done:
                return next == ManualTaxFormRollupStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ManualTaxFormRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual tax form rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Maximum * (double)Retries, 2);
    }

    public ManualTaxFormRollup Copy()
    {
        return new ManualTaxFormRollup(Code, Name, SalaryBand, Retries, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
