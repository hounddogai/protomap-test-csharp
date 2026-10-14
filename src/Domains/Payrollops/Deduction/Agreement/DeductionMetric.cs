namespace Acme.Workforce.Payrollops.Deduction.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction metric is in its life cycle.</summary>
public enum DeductionMetricStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A deduction metric as the deduction team keeps it.</summary>
public sealed class DeductionMetric(string code, string name, int limit, string source, string title, string grade, double minimum, DeductionMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public string Source { get; set; } = source;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public double Minimum { get; set; } = minimum;
    public DeductionMetricStatus Status { get; set; } = status;

    /// <summary>Starts a deduction metric in status draft.</summary>
    public static DeductionMetric NewDeductionMetric(string code, string name)
    {
        return new DeductionMetric(code, name, 0, "", "", "", 0.0, DeductionMetricStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionMetricStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionMetricStatus next)
    {
        switch (Status)
        {
            case DeductionMetricStatus.Draft:
                return next == DeductionMetricStatus.Suspended || next == DeductionMetricStatus.Closed;
            case DeductionMetricStatus.Pending:
                switch (next)
                {
                    case DeductionMetricStatus.Active:
                        return true;
                }
                return false;
            case DeductionMetricStatus.Active:
                switch (next)
                {
                    case DeductionMetricStatus.Suspended:
                    case DeductionMetricStatus.Closed:
                        return true;
                }
                return false;
            case DeductionMetricStatus.Suspended:
                switch (next)
                {
                    case DeductionMetricStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeductionMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"deduction metric"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Limit * Minimum, 2);
    }

    public DeductionMetric Copy()
    {
        return new DeductionMetric(Code, Name, Limit, Source, Title, Grade, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
