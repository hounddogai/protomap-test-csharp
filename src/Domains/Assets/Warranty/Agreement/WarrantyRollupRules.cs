namespace Acme.Workforce.Assets.Warranty.Agreement;

/// <summary>A rule a warranty rollup must pass. Check returns its problems, or none.</summary>
public interface IWarrantyRollupRule
{
    List<string> Check(WarrantyRollup item);
}

public sealed record WarrantyRollupCodeLengthRule(int MaxLength) : IWarrantyRollupRule
{
    public List<string> Check(WarrantyRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyRollupAttemptsRangeRule(int Min, int Max) : IWarrantyRollupRule
{
    public List<string> Check(WarrantyRollup item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WarrantyRollupExternalRefLengthRule(int MaxLength) : IWarrantyRollupRule
{
    public List<string> Check(WarrantyRollup item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyRollupRequiredExternalRefRule(string Message) : IWarrantyRollupRule
{
    public List<string> Check(WarrantyRollup item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty rollup.</summary>
public sealed record WarrantyRollupValidator(List<IWarrantyRollupRule> Rules)
{
    public List<string> Validate(WarrantyRollup item)
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

    public void RequireValid(WarrantyRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyRollup> items)
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

/// <summary>Functions of warranty rollup rules.</summary>
public static class WarrantyRollupRules
{
    public static WarrantyRollupValidator DefaultWarrantyRollupValidator()
    {
        return new WarrantyRollupValidator(
            new List<IWarrantyRollupRule>
            {
                new WarrantyRollupCodeLengthRule(255),
                new WarrantyRollupAttemptsRangeRule(80, 1000),
                new WarrantyRollupExternalRefLengthRule(80),
                new WarrantyRollupRequiredExternalRefRule("warranty rollup needs this field"),
            });
    }
}
