namespace Acme.Workforce.Payrollops.TaxForm.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted tax form exception is in its life cycle.</summary>
public enum WeightedTaxFormExceptionStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A weighted tax form exception. Its code is unique within a tenant.</summary>
public sealed class WeightedTaxFormException(string code, string name, string salaryBand, double margin, string summary, int expiresAt, string skill, WeightedTaxFormExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public double Margin { get; set; } = margin;
    public string Summary { get; set; } = summary;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Skill { get; set; } = skill;
    public WeightedTaxFormExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a weighted tax form exception in status planned.</summary>
    public static WeightedTaxFormException NewWeightedTaxFormException(string code, string name)
    {
        return new WeightedTaxFormException(code, name, "", 0.0, "", 0, "", WeightedTaxFormExceptionStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedTaxFormExceptionStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedTaxFormExceptionStatus next)
    {
        switch (Status)
        {
            case WeightedTaxFormExceptionStatus.Planned:
                return next == WeightedTaxFormExceptionStatus.Completed || next == WeightedTaxFormExceptionStatus.Cancelled;
            case WeightedTaxFormExceptionStatus.Scheduled:
                switch (next)
                {
                    case WeightedTaxFormExceptionStatus.Completed:
                        return true;
                }
                return false;
            case WeightedTaxFormExceptionStatus.InProgress:
                switch (next)
                {
                    case WeightedTaxFormExceptionStatus.Completed:
                    case WeightedTaxFormExceptionStatus.Cancelled:
                        return true;
                }
                return false;
            case WeightedTaxFormExceptionStatus.Completed:
                return next == WeightedTaxFormExceptionStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(WeightedTaxFormExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted tax form exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)ExpiresAt + Margin, 2);
    }

    public WeightedTaxFormException Copy()
    {
        return new WeightedTaxFormException(Code, Name, SalaryBand, Margin, Summary, ExpiresAt, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
