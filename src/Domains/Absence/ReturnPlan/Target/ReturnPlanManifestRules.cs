namespace Acme.Workforce.Absence.ReturnPlan.Target;

/// <summary>A rule a return plan manifest must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanManifestRule
{
    List<string> Check(ReturnPlanManifest item);
}

public sealed record ReturnPlanManifestLabelLengthRule(int MaxLength) : IReturnPlanManifestRule
{
    public List<string> Check(ReturnPlanManifest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanManifestCodePrefixRule(string Prefix) : IReturnPlanManifestRule
{
    public List<string> Check(ReturnPlanManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanManifestRequiredTeamRule(string Message) : IReturnPlanManifestRule
{
    public List<string> Check(ReturnPlanManifest item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanManifestStartsAtRangeRule(int Min, int Max) : IReturnPlanManifestRule
{
    public List<string> Check(ReturnPlanManifest item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanManifestTeamLengthRule(int MaxLength) : IReturnPlanManifestRule
{
    public List<string> Check(ReturnPlanManifest item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan manifest.</summary>
public sealed record ReturnPlanManifestValidator(List<IReturnPlanManifestRule> Rules)
{
    public List<string> Validate(ReturnPlanManifest item)
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

    public void RequireValid(ReturnPlanManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanManifest> items)
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

/// <summary>Functions of return plan manifest rules.</summary>
public static class ReturnPlanManifestRules
{
    public static ReturnPlanManifestValidator DefaultReturnPlanManifestValidator()
    {
        return new ReturnPlanManifestValidator(
            new List<IReturnPlanManifestRule>
            {
                new ReturnPlanManifestLabelLengthRule(32),
                new ReturnPlanManifestCodePrefixRule("X"),
                new ReturnPlanManifestRequiredTeamRule("return plan manifest needs an entry"),
                new ReturnPlanManifestStartsAtRangeRule(255, 86400),
                new ReturnPlanManifestTeamLengthRule(1000),
            });
    }
}
