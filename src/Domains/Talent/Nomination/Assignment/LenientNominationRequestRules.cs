namespace Acme.Workforce.Talent.Nomination.Assignment;

/// <summary>A rule a lenient nomination request must pass. Check returns its problems, or none.</summary>
public interface ILenientNominationRequestRule
{
    List<string> Check(LenientNominationRequest item);
}

public sealed record LenientNominationRequestBudgetRangeRule(double Min, double Max) : ILenientNominationRequestRule
{
    public List<string> Check(LenientNominationRequest item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientNominationRequestRequiredUnitRule(string Message) : ILenientNominationRequestRule
{
    public List<string> Check(LenientNominationRequest item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientNominationRequestRequiredLabelRule(string Message) : ILenientNominationRequestRule
{
    public List<string> Check(LenientNominationRequest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientNominationRequestUnitLengthRule(int MaxLength) : ILenientNominationRequestRule
{
    public List<string> Check(LenientNominationRequest item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient nomination request.</summary>
public sealed record LenientNominationRequestValidator(List<ILenientNominationRequestRule> Rules)
{
    public List<string> Validate(LenientNominationRequest item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(LenientNominationRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientNominationRequest> items)
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

/// <summary>Functions of lenient nomination request rules.</summary>
public static class LenientNominationRequestRules
{
    public static LenientNominationRequestValidator DefaultLenientNominationRequestValidator()
    {
        return new LenientNominationRequestValidator(
            new List<ILenientNominationRequestRule>
            {
                new LenientNominationRequestBudgetRangeRule(4.9, 2325.7),
                new LenientNominationRequestRequiredUnitRule("lenient nomination request needs a value"),
                new LenientNominationRequestRequiredLabelRule("lenient nomination request needs this field"),
                new LenientNominationRequestUnitLengthRule(32),
            });
    }
}
