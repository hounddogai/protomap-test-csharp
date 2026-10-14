namespace Acme.Workforce.Absence.Policy.Rollup;

/// <summary>A rule a global policy bundle must pass. Check returns its problems, or none.</summary>
public interface IGlobalPolicyBundleRule
{
    List<string> Check(GlobalPolicyBundle item);
}

public sealed record GlobalPolicyBundleUnitLengthRule(int MaxLength) : IGlobalPolicyBundleRule
{
    public List<string> Check(GlobalPolicyBundle item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPolicyBundleCodeLengthRule(int MaxLength) : IGlobalPolicyBundleRule
{
    public List<string> Check(GlobalPolicyBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPolicyBundleSequenceRangeRule(int Min, int Max) : IGlobalPolicyBundleRule
{
    public List<string> Check(GlobalPolicyBundle item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPolicyBundleRevisionRangeRule(int Min, int Max) : IGlobalPolicyBundleRule
{
    public List<string> Check(GlobalPolicyBundle item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPolicyBundleRequiredLabelRule(string Message) : IGlobalPolicyBundleRule
{
    public List<string> Check(GlobalPolicyBundle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global policy bundle.</summary>
public sealed record GlobalPolicyBundleValidator(List<IGlobalPolicyBundleRule> Rules)
{
    public List<string> Validate(GlobalPolicyBundle item)
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

    public void RequireValid(GlobalPolicyBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalPolicyBundle> items)
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

/// <summary>Functions of global policy bundle rules.</summary>
public static class GlobalPolicyBundleRules
{
    public static GlobalPolicyBundleValidator DefaultGlobalPolicyBundleValidator()
    {
        return new GlobalPolicyBundleValidator(
            new List<IGlobalPolicyBundleRule>
            {
                new GlobalPolicyBundleUnitLengthRule(86400),
                new GlobalPolicyBundleCodeLengthRule(80),
                new GlobalPolicyBundleSequenceRangeRule(255, 3600),
                new GlobalPolicyBundleRevisionRangeRule(80, 80),
                new GlobalPolicyBundleRequiredLabelRule("global policy bundle needs a value"),
            });
    }
}
