namespace Acme.Workforce.Compensation.PayBand.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default pay band estimate must pass. Check returns its problems, or none.</summary>
public interface IDefaultPayBandEstimateRule
{
    List<string> Check(DefaultPayBandEstimate item);
}

public sealed record DefaultPayBandEstimateNameLengthRule(int MaxLength) : IDefaultPayBandEstimateRule
{
    public List<string> Check(DefaultPayBandEstimate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayBandEstimateRequiredCodeRule(string Message) : IDefaultPayBandEstimateRule
{
    public List<string> Check(DefaultPayBandEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayBandEstimateAllowedDescriptionRule(List<string> Allowed) : IDefaultPayBandEstimateRule
{
    public List<string> Check(DefaultPayBandEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayBandEstimateCodeLengthRule(int MaxLength) : IDefaultPayBandEstimateRule
{
    public List<string> Check(DefaultPayBandEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default pay band estimate.</summary>
public sealed record DefaultPayBandEstimateValidator(List<IDefaultPayBandEstimateRule> Rules)
{
    public List<string> Validate(DefaultPayBandEstimate item)
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

    public void RequireValid(DefaultPayBandEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPayBandEstimate> items)
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

/// <summary>Functions of default pay band estimate rules.</summary>
public static class DefaultPayBandEstimateRules
{
    public static DefaultPayBandEstimateValidator DefaultDefaultPayBandEstimateValidator()
    {
        return new DefaultPayBandEstimateValidator(
            new List<IDefaultPayBandEstimateRule>
            {
                new DefaultPayBandEstimateNameLengthRule(80),
                new DefaultPayBandEstimateRequiredCodeRule("default pay band estimate needs a value"),
                new DefaultPayBandEstimateAllowedDescriptionRule(new List<string> { "primary", "regional" }),
                new DefaultPayBandEstimateCodeLengthRule(3600),
            });
    }
}
