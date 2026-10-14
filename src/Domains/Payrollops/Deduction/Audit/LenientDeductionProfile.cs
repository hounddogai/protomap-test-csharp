namespace Acme.Workforce.Payrollops.Deduction.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient deduction profile is in its life cycle.</summary>
public enum LenientDeductionProfileStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A lenient deduction profile as the deduction team keeps it.</summary>
public sealed class LenientDeductionProfile(string code, string name, double ceiling, double ratio, string summary, string category, LenientDeductionProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public double Ratio { get; set; } = ratio;
    public string Summary { get; set; } = summary;
    public string Category { get; set; } = category;
    public LenientDeductionProfileStatus Status { get; set; } = status;

    /// <summary>Starts a lenient deduction profile in status draft.</summary>
    public static LenientDeductionProfile NewLenientDeductionProfile(string code, string name)
    {
        return new LenientDeductionProfile(code, name, 0.0, 0.0, "", "", LenientDeductionProfileStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientDeductionProfileStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientDeductionProfileStatus next)
    {
        switch (Status)
        {
            case LenientDeductionProfileStatus.Draft:
                return next == LenientDeductionProfileStatus.Closed;
            case LenientDeductionProfileStatus.Pending:
                switch (next)
                {
                    case LenientDeductionProfileStatus.Suspended:
                    case LenientDeductionProfileStatus.Closed:
                        return true;
                }
                return false;
            case LenientDeductionProfileStatus.Active:
                return next == LenientDeductionProfileStatus.Suspended;
            case LenientDeductionProfileStatus.Suspended:
                switch (next)
                {
                    case LenientDeductionProfileStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientDeductionProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient deduction profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedValue()
    {
        return Math.Round(Ceiling - Ratio, 2);
    }

    public LenientDeductionProfile Copy()
    {
        return new LenientDeductionProfile(Code, Name, Ceiling, Ratio, Summary, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
