namespace Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>A rule a career path group must pass. Check returns its problems, or none.</summary>
public interface ICareerPathGroupRule
{
    List<string> Check(CareerPathGroup item);
}

public sealed record CareerPathGroupRequiredCodeRule(string Message) : ICareerPathGroupRule
{
    public List<string> Check(CareerPathGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathGroupPhoneLengthRule(int MaxLength) : ICareerPathGroupRule
{
    public List<string> Check(CareerPathGroup item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathGroupExpiresAtRangeRule(int Min, int Max) : ICareerPathGroupRule
{
    public List<string> Check(CareerPathGroup item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathGroupRequiredLabelRule(string Message) : ICareerPathGroupRule
{
    public List<string> Check(CareerPathGroup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathGroupCodePrefixRule(string Prefix) : ICareerPathGroupRule
{
    public List<string> Check(CareerPathGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path group.</summary>
public sealed record CareerPathGroupValidator(List<ICareerPathGroupRule> Rules)
{
    public List<string> Validate(CareerPathGroup item)
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

    public void RequireValid(CareerPathGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathGroup> items)
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

/// <summary>Functions of career path group rules.</summary>
public static class CareerPathGroupRules
{
    public static CareerPathGroupValidator DefaultCareerPathGroupValidator()
    {
        return new CareerPathGroupValidator(
            new List<ICareerPathGroupRule>
            {
                new CareerPathGroupRequiredCodeRule("career path group needs an entry"),
                new CareerPathGroupPhoneLengthRule(64),
                new CareerPathGroupExpiresAtRangeRule(32, 120),
                new CareerPathGroupRequiredLabelRule("career path group needs a value"),
                new CareerPathGroupCodePrefixRule("REF"),
            });
    }
}
