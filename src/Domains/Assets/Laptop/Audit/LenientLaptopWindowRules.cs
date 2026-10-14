namespace Acme.Workforce.Assets.Laptop.Audit;

/// <summary>A rule a lenient laptop window must pass. Check returns its problems, or none.</summary>
public interface ILenientLaptopWindowRule
{
    List<string> Check(LenientLaptopWindow item);
}

public sealed record LenientLaptopWindowScoreRangeRule(double Min, double Max) : ILenientLaptopWindowRule
{
    public List<string> Check(LenientLaptopWindow item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientLaptopWindowRequiredNameRule(string Message) : ILenientLaptopWindowRule
{
    public List<string> Check(LenientLaptopWindow item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientLaptopWindowCodePrefixRule(string Prefix) : ILenientLaptopWindowRule
{
    public List<string> Check(LenientLaptopWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientLaptopWindowNameLengthRule(int MaxLength) : ILenientLaptopWindowRule
{
    public List<string> Check(LenientLaptopWindow item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient laptop window.</summary>
public sealed record LenientLaptopWindowValidator(List<ILenientLaptopWindowRule> Rules)
{
    public List<string> Validate(LenientLaptopWindow item)
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

    public void RequireValid(LenientLaptopWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientLaptopWindow> items)
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

/// <summary>Functions of lenient laptop window rules.</summary>
public static class LenientLaptopWindowRules
{
    public static LenientLaptopWindowValidator DefaultLenientLaptopWindowValidator()
    {
        return new LenientLaptopWindowValidator(
            new List<ILenientLaptopWindowRule>
            {
                new LenientLaptopWindowScoreRangeRule(0.1, 607.5),
                new LenientLaptopWindowRequiredNameRule("lenient laptop window needs this field"),
                new LenientLaptopWindowCodePrefixRule("X"),
                new LenientLaptopWindowNameLengthRule(120),
            });
    }
}
