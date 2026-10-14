namespace Acme.Workforce.Orgdesign.Position.Bundle;

/// <summary>A rule a tiered position template must pass. Check returns its problems, or none.</summary>
public interface ITieredPositionTemplateRule
{
    List<string> Check(TieredPositionTemplate item);
}

public sealed record TieredPositionTemplateRequiredChannelRule(string Message) : ITieredPositionTemplateRule
{
    public List<string> Check(TieredPositionTemplate item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPositionTemplateCodePrefixRule(string Prefix) : ITieredPositionTemplateRule
{
    public List<string> Check(TieredPositionTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPositionTemplateWindowDaysRangeRule(int Min, int Max) : ITieredPositionTemplateRule
{
    public List<string> Check(TieredPositionTemplate item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPositionTemplateChecksumLengthRule(int MaxLength) : ITieredPositionTemplateRule
{
    public List<string> Check(TieredPositionTemplate item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPositionTemplateChannelLengthRule(int MaxLength) : ITieredPositionTemplateRule
{
    public List<string> Check(TieredPositionTemplate item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered position template.</summary>
public sealed record TieredPositionTemplateValidator(List<ITieredPositionTemplateRule> Rules)
{
    public List<string> Validate(TieredPositionTemplate item)
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

    public void RequireValid(TieredPositionTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPositionTemplate> items)
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

/// <summary>Functions of tiered position template rules.</summary>
public static class TieredPositionTemplateRules
{
    public static TieredPositionTemplateValidator DefaultTieredPositionTemplateValidator()
    {
        return new TieredPositionTemplateValidator(
            new List<ITieredPositionTemplateRule>
            {
                new TieredPositionTemplateRequiredChannelRule("tiered position template needs an entry"),
                new TieredPositionTemplateCodePrefixRule("REF"),
                new TieredPositionTemplateWindowDaysRangeRule(32, 64),
                new TieredPositionTemplateChecksumLengthRule(255),
                new TieredPositionTemplateChannelLengthRule(16),
            });
    }
}
