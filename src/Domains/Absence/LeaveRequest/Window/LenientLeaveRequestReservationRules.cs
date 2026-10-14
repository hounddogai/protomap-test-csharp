namespace Acme.Workforce.Absence.LeaveRequest.Window;

/// <summary>A rule a lenient leave request reservation must pass. Check returns its problems, or none.</summary>
public interface ILenientLeaveRequestReservationRule
{
    List<string> Check(LenientLeaveRequestReservation item);
}

public sealed record LenientLeaveRequestReservationTenureMonthsRangeRule(int Min, int Max) : ILenientLeaveRequestReservationRule
{
    public List<string> Check(LenientLeaveRequestReservation item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientLeaveRequestReservationRequiredCodeRule(string Message) : ILenientLeaveRequestReservationRule
{
    public List<string> Check(LenientLeaveRequestReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientLeaveRequestReservationCodePrefixRule(string Prefix) : ILenientLeaveRequestReservationRule
{
    public List<string> Check(LenientLeaveRequestReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient leave request reservation.</summary>
public sealed record LenientLeaveRequestReservationValidator(List<ILenientLeaveRequestReservationRule> Rules)
{
    public List<string> Validate(LenientLeaveRequestReservation item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(LenientLeaveRequestReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientLeaveRequestReservation> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of lenient leave request reservation rules.</summary>
public static class LenientLeaveRequestReservationRules
{
    public static LenientLeaveRequestReservationValidator DefaultLenientLeaveRequestReservationValidator()
    {
        return new LenientLeaveRequestReservationValidator(
            new List<ILenientLeaveRequestReservationRule>
            {
                new LenientLeaveRequestReservationTenureMonthsRangeRule(16, 1000),
                new LenientLeaveRequestReservationRequiredCodeRule(
                    "lenient leave request reservation needs this field"),
                new LenientLeaveRequestReservationCodePrefixRule("OPS"),
            });
    }
}
