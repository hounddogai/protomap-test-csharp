namespace Acme.Workforce.Orgdesign.Position.Profile;

/// <summary>A rule a global position rollup must pass. Check returns its problems, or none.</summary>
public interface IGlobalPositionRollupRule
{
    List<string> Check(GlobalPositionRollup item);
}

public sealed record GlobalPositionRollupUnitLengthRule(int MaxLength) : IGlobalPositionRollupRule
{
    public List<string> Check(GlobalPositionRollup item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPositionRollupApprovedGuardRule(double Limit) : IGlobalPositionRollupRule
{
    public List<string> Check(GlobalPositionRollup item)
    {
        if (item.Approved && item.Ratio > Limit)
        {
            return new List<string> { $"ratio {item.Ratio} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPositionRollupAmountRangeRule(double Min, double Max) : IGlobalPositionRollupRule
{
    public List<string> Check(GlobalPositionRollup item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global position rollup.</summary>
public sealed record GlobalPositionRollupValidator(List<IGlobalPositionRollupRule> Rules)
{
    public List<string> Validate(GlobalPositionRollup item)
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

    public void RequireValid(GlobalPositionRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalPositionRollup> items)
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

/// <summary>Functions of global position rollup rules.</summary>
public static class GlobalPositionRollupRules
{
    public static GlobalPositionRollupValidator DefaultGlobalPositionRollupValidator()
    {
        return new GlobalPositionRollupValidator(
            new List<IGlobalPositionRollupRule>
            {
                new GlobalPositionRollupUnitLengthRule(64),
                new GlobalPositionRollupApprovedGuardRule(4.1),
                new GlobalPositionRollupAmountRangeRule(8.5, 1296.9),
            });
    }
}
