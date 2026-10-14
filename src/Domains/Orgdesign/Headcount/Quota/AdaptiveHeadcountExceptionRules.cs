namespace Acme.Workforce.Orgdesign.Headcount.Quota;

/// <summary>A rule a adaptive headcount exception must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveHeadcountExceptionRule
{
    List<string> Check(AdaptiveHeadcountException item);
}

public sealed record AdaptiveHeadcountExceptionRequiredRegionRule(string Message) : IAdaptiveHeadcountExceptionRule
{
    public List<string> Check(AdaptiveHeadcountException item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveHeadcountExceptionRequiredCodeRule(string Message) : IAdaptiveHeadcountExceptionRule
{
    public List<string> Check(AdaptiveHeadcountException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveHeadcountExceptionCodeLengthRule(int MaxLength) : IAdaptiveHeadcountExceptionRule
{
    public List<string> Check(AdaptiveHeadcountException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive headcount exception.</summary>
public sealed record AdaptiveHeadcountExceptionValidator(List<IAdaptiveHeadcountExceptionRule> Rules)
{
    public List<string> Validate(AdaptiveHeadcountException item)
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

    public void RequireValid(AdaptiveHeadcountException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveHeadcountException> items)
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

/// <summary>Functions of adaptive headcount exception rules.</summary>
public static class AdaptiveHeadcountExceptionRules
{
    public static AdaptiveHeadcountExceptionValidator DefaultAdaptiveHeadcountExceptionValidator()
    {
        return new AdaptiveHeadcountExceptionValidator(
            new List<IAdaptiveHeadcountExceptionRule>
            {
                new AdaptiveHeadcountExceptionRequiredRegionRule("adaptive headcount exception needs an entry"),
                new AdaptiveHeadcountExceptionRequiredCodeRule("adaptive headcount exception needs a value"),
                new AdaptiveHeadcountExceptionCodeLengthRule(32),
            });
    }
}
