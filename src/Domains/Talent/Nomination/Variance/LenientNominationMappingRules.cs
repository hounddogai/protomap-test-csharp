namespace Acme.Workforce.Talent.Nomination.Variance;

/// <summary>A rule a lenient nomination mapping must pass. Check returns its problems, or none.</summary>
public interface ILenientNominationMappingRule
{
    List<string> Check(LenientNominationMapping item);
}

public sealed record LenientNominationMappingCurrencyLengthRule(int MaxLength) : ILenientNominationMappingRule
{
    public List<string> Check(LenientNominationMapping item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientNominationMappingRequiredNameRule(string Message) : ILenientNominationMappingRule
{
    public List<string> Check(LenientNominationMapping item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientNominationMappingCodeLengthRule(int MaxLength) : ILenientNominationMappingRule
{
    public List<string> Check(LenientNominationMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientNominationMappingBudgetRangeRule(double Min, double Max) : ILenientNominationMappingRule
{
    public List<string> Check(LenientNominationMapping item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientNominationMappingRequiredSourceRule(string Message) : ILenientNominationMappingRule
{
    public List<string> Check(LenientNominationMapping item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientNominationMappingArchivedGuardRule(int Limit) : ILenientNominationMappingRule
{
    public List<string> Check(LenientNominationMapping item)
    {
        if (item.Archived && item.WindowDays > Limit)
        {
            return new List<string> { $"window days {item.WindowDays} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient nomination mapping.</summary>
public sealed record LenientNominationMappingValidator(List<ILenientNominationMappingRule> Rules)
{
    public List<string> Validate(LenientNominationMapping item)
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

    public void RequireValid(LenientNominationMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientNominationMapping> items)
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

/// <summary>Functions of lenient nomination mapping rules.</summary>
public static class LenientNominationMappingRules
{
    public static LenientNominationMappingValidator DefaultLenientNominationMappingValidator()
    {
        return new LenientNominationMappingValidator(
            new List<ILenientNominationMappingRule>
            {
                new LenientNominationMappingCurrencyLengthRule(86400),
                new LenientNominationMappingRequiredNameRule("lenient nomination mapping needs a value"),
                new LenientNominationMappingCodeLengthRule(32),
                new LenientNominationMappingBudgetRangeRule(4.8, 126.9),
                new LenientNominationMappingRequiredSourceRule("lenient nomination mapping needs an entry"),
                new LenientNominationMappingArchivedGuardRule(32),
            });
    }
}
