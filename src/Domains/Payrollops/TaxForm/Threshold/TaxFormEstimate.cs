namespace Acme.Workforce.Payrollops.TaxForm.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form estimate is in its life cycle.</summary>
public enum TaxFormEstimateStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a tax form estimate and its current status.</summary>
public sealed class TaxFormEstimate(string code, string title, int attempts, double ratio, string employeeId, int priority, TaxFormEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public double Ratio { get; set; } = ratio;
    public string EmployeeId { get; set; } = employeeId;
    public int Priority { get; set; } = priority;
    public TaxFormEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a tax form estimate in status created.</summary>
    public static TaxFormEstimate NewTaxFormEstimate(string code, string title)
    {
        return new TaxFormEstimate(code, title, 0, 0.0, "", 0, TaxFormEstimateStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormEstimateStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormEstimateStatus next)
    {
        switch (Status)
        {
            case TaxFormEstimateStatus.Created:
                switch (next)
                {
                    case TaxFormEstimateStatus.Retired:
                        return true;
                }
                return false;
            case TaxFormEstimateStatus.Validated:
                return next == TaxFormEstimateStatus.Published || next == TaxFormEstimateStatus.Retired;
            case TaxFormEstimateStatus.Published:
                switch (next)
                {
                    case TaxFormEstimateStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedExposure()
    {
        return Math.Round(Ratio * (double)Attempts, 2);
    }

    public TaxFormEstimate Copy()
    {
        return new TaxFormEstimate(Code, Title, Attempts, Ratio, EmployeeId, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
