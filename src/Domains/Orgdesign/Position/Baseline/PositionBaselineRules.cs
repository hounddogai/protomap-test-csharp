namespace Acme.Workforce.Orgdesign.Position.Baseline;

/// <summary>A rule a position baseline must pass. Check returns its problems, or none.</summary>
public interface IPositionBaselineRule
{
    List<string> Check(PositionBaseline item);
}

public sealed record PositionBaselineCodeLengthRule(int MaxLength) : IPositionBaselineRule
{
    public List<string> Check(PositionBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionBaselineCodePrefixRule(string Prefix) : IPositionBaselineRule
{
    public List<string> Check(PositionBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PositionBaselineRequiredSkillRule(string Message) : IPositionBaselineRule
{
    public List<string> Check(PositionBaseline item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionBaselineAmountRangeRule(double Min, double Max) : IPositionBaselineRule
{
    public List<string> Check(PositionBaseline item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position baseline.</summary>
public sealed record PositionBaselineValidator(List<IPositionBaselineRule> Rules)
{
    public List<string> Validate(PositionBaseline item)
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

    public void RequireValid(PositionBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionBaseline> items)
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

/// <summary>Functions of position baseline rules.</summary>
public static class PositionBaselineRules
{
    public static PositionBaselineValidator DefaultPositionBaselineValidator()
    {
        return new PositionBaselineValidator(
            new List<IPositionBaselineRule>
            {
                new PositionBaselineCodeLengthRule(255),
                new PositionBaselineCodePrefixRule("OPS"),
                new PositionBaselineRequiredSkillRule("position baseline needs a value"),
                new PositionBaselineAmountRangeRule(0.8, 2531.2),
            });
    }
}
