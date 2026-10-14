namespace Acme.Workforce.Orgdesign.Span.Policy;

/// <summary>A rule a global span rollup must pass. Check returns its problems, or none.</summary>
public interface IGlobalSpanRollupRule
{
    List<string> Check(GlobalSpanRollup item);
}

public sealed record GlobalSpanRollupLabelLengthRule(int MaxLength) : IGlobalSpanRollupRule
{
    public List<string> Check(GlobalSpanRollup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalSpanRollupQuantityRangeRule(int Min, int Max) : IGlobalSpanRollupRule
{
    public List<string> Check(GlobalSpanRollup item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalSpanRollupRequiredCodeRule(string Message) : IGlobalSpanRollupRule
{
    public List<string> Check(GlobalSpanRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalSpanRollupRequiredLabelRule(string Message) : IGlobalSpanRollupRule
{
    public List<string> Check(GlobalSpanRollup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalSpanRollupRequiredChannelRule(string Message) : IGlobalSpanRollupRule
{
    public List<string> Check(GlobalSpanRollup item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalSpanRollupCodeLengthRule(int MaxLength) : IGlobalSpanRollupRule
{
    public List<string> Check(GlobalSpanRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global span rollup.</summary>
public sealed record GlobalSpanRollupValidator(List<IGlobalSpanRollupRule> Rules)
{
    public List<string> Validate(GlobalSpanRollup item)
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

    public void RequireValid(GlobalSpanRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalSpanRollup> items)
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

/// <summary>Functions of global span rollup rules.</summary>
public static class GlobalSpanRollupRules
{
    public static GlobalSpanRollupValidator DefaultGlobalSpanRollupValidator()
    {
        return new GlobalSpanRollupValidator(
            new List<IGlobalSpanRollupRule>
            {
                new GlobalSpanRollupLabelLengthRule(16),
                new GlobalSpanRollupQuantityRangeRule(32, 255),
                new GlobalSpanRollupRequiredCodeRule("global span rollup needs a value"),
                new GlobalSpanRollupRequiredLabelRule("global span rollup needs a value"),
                new GlobalSpanRollupRequiredChannelRule("global span rollup needs a value"),
                new GlobalSpanRollupCodeLengthRule(1000),
            });
    }
}
