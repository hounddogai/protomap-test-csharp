namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient nomination rule is in its life cycle.</summary>
public enum LenientNominationRuleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a lenient nomination rule and its current status.</summary>
public sealed class LenientNominationRule(string code, string name, double cost, string currency, double ceiling, double ratio, double budget, LenientNominationRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public string Currency { get; set; } = currency;
    public double Ceiling { get; set; } = ceiling;
    public double Ratio { get; set; } = ratio;
    public double Budget { get; set; } = budget;
    public LenientNominationRuleStatus Status { get; set; } = status;

    /// <summary>Starts a lenient nomination rule in status draft.</summary>
    public static LenientNominationRule NewLenientNominationRule(string code, string name)
    {
        return new LenientNominationRule(code, name, 0.0, "", 0.0, 0.0, 0.0, LenientNominationRuleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientNominationRuleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientNominationRuleStatus next)
    {
        switch (Status)
        {
            case LenientNominationRuleStatus.Draft:
                return next == LenientNominationRuleStatus.Closed;
            case LenientNominationRuleStatus.Pending:
                return next == LenientNominationRuleStatus.Active || next == LenientNominationRuleStatus.Closed;
            case LenientNominationRuleStatus.Active:
                switch (next)
                {
                    case LenientNominationRuleStatus.Suspended:
                    case LenientNominationRuleStatus.Closed:
                        return true;
                }
                return false;
            case LenientNominationRuleStatus.Suspended:
                return next == LenientNominationRuleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LenientNominationRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient nomination rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient nomination rule"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Budget + Ratio, 2);
    }

    public LenientNominationRule Copy()
    {
        return new LenientNominationRule(Code, Name, Cost, Currency, Ceiling, Ratio, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
