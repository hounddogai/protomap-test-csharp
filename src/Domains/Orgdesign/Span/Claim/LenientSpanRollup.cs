namespace Acme.Workforce.Orgdesign.Span.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient span rollup is in its life cycle.</summary>
public enum LenientSpanRollupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A lenient span rollup as the span team keeps it.</summary>
public sealed class LenientSpanRollup(string code, string name, string employeeId, int leaveDays, string category, string country, int attempts, double budget, LenientSpanRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public int LeaveDays { get; set; } = leaveDays;
    public string Category { get; set; } = category;
    public string Country { get; set; } = country;
    public int Attempts { get; set; } = attempts;
    public double Budget { get; set; } = budget;
    public LenientSpanRollupStatus Status { get; set; } = status;

    /// <summary>Starts a lenient span rollup in status created.</summary>
    public static LenientSpanRollup NewLenientSpanRollup(string code, string name)
    {
        return new LenientSpanRollup(code, name, "", 0, "", "", 0, 0.0, LenientSpanRollupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientSpanRollupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientSpanRollupStatus next)
    {
        switch (Status)
        {
            case LenientSpanRollupStatus.Created:
                return next == LenientSpanRollupStatus.Validated;
            case LenientSpanRollupStatus.Validated:
                switch (next)
                {
                    case LenientSpanRollupStatus.Published:
                        return true;
                }
                return false;
            case LenientSpanRollupStatus.Published:
                return next == LenientSpanRollupStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LenientSpanRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient span rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient span rollup"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)LeaveDays - (double)Attempts, 2);
    }

    public LenientSpanRollup Copy()
    {
        return new LenientSpanRollup(Code, Name, EmployeeId, LeaveDays, Category, Country, Attempts, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
