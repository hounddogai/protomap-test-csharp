namespace Acme.Workforce.Payrollops.Deduction.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction variance is in its life cycle.</summary>
public enum DeductionVarianceStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One deduction variance, with what the planners need to act on it.</summary>
public sealed class DeductionVariance(string code, string title, double threshold, string skill, string name, int version, double budget, double amount, DeductionVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public string Skill { get; set; } = skill;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public double Budget { get; set; } = budget;
    public double Amount { get; set; } = amount;
    public DeductionVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a deduction variance in status idle.</summary>
    public static DeductionVariance NewDeductionVariance(string code, string title)
    {
        return new DeductionVariance(code, title, 0.0, "", "", 0, 0.0, 0.0, DeductionVarianceStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionVarianceStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionVarianceStatus next)
    {
        switch (Status)
        {
            case DeductionVarianceStatus.Idle:
                return next == DeductionVarianceStatus.Stopped;
            case DeductionVarianceStatus.Warming:
                switch (next)
                {
                    case DeductionVarianceStatus.Ready:
                    case DeductionVarianceStatus.Draining:
                        return true;
                }
                return false;
            case DeductionVarianceStatus.Ready:
                return next == DeductionVarianceStatus.Draining || next == DeductionVarianceStatus.Stopped;
            case DeductionVarianceStatus.Draining:
                return next == DeductionVarianceStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(DeductionVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"deduction variance"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Threshold - (double)Version, 2);
    }

    public DeductionVariance Copy()
    {
        return new DeductionVariance(Code, Title, Threshold, Skill, Name, Version, Budget, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
