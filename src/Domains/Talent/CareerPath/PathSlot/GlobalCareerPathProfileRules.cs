namespace Acme.Workforce.Talent.CareerPath.PathSlot;

/// <summary>A rule a global career path profile must pass. Check returns its problems, or none.</summary>
public interface IGlobalCareerPathProfileRule
{
    List<string> Check(GlobalCareerPathProfile item);
}

public sealed record GlobalCareerPathProfileCodePrefixRule(string Prefix) : IGlobalCareerPathProfileRule
{
    public List<string> Check(GlobalCareerPathProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalCareerPathProfileRequiredJobCodeRule(string Message) : IGlobalCareerPathProfileRule
{
    public List<string> Check(GlobalCareerPathProfile item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalCareerPathProfileJobCodeLengthRule(int MaxLength) : IGlobalCareerPathProfileRule
{
    public List<string> Check(GlobalCareerPathProfile item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalCareerPathProfileNameLengthRule(int MaxLength) : IGlobalCareerPathProfileRule
{
    public List<string> Check(GlobalCareerPathProfile item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global career path profile.</summary>
public sealed record GlobalCareerPathProfileValidator(List<IGlobalCareerPathProfileRule> Rules)
{
    public List<string> Validate(GlobalCareerPathProfile item)
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

    public void RequireValid(GlobalCareerPathProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalCareerPathProfile> items)
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

/// <summary>Functions of global career path profile rules.</summary>
public static class GlobalCareerPathProfileRules
{
    public static GlobalCareerPathProfileValidator DefaultGlobalCareerPathProfileValidator()
    {
        return new GlobalCareerPathProfileValidator(
            new List<IGlobalCareerPathProfileRule>
            {
                new GlobalCareerPathProfileCodePrefixRule("REF"),
                new GlobalCareerPathProfileRequiredJobCodeRule("global career path profile needs an entry"),
                new GlobalCareerPathProfileJobCodeLengthRule(1000),
                new GlobalCareerPathProfileNameLengthRule(86400),
            });
    }
}
