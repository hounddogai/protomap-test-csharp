namespace Acme.Workforce.Payrollops.RetroPay.PayBudget;

/// <summary>A rule a retro pay template must pass. Check returns its problems, or none.</summary>
public interface IRetroPayTemplateRule
{
    List<string> Check(RetroPayTemplate item);
}

public sealed record RetroPayTemplateNameLengthRule(int MaxLength) : IRetroPayTemplateRule
{
    public List<string> Check(RetroPayTemplate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayTemplateCodePrefixRule(string Prefix) : IRetroPayTemplateRule
{
    public List<string> Check(RetroPayTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayTemplateRequiredDescriptionRule(string Message) : IRetroPayTemplateRule
{
    public List<string> Check(RetroPayTemplate item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay template.</summary>
public sealed record RetroPayTemplateValidator(List<IRetroPayTemplateRule> Rules)
{
    public List<string> Validate(RetroPayTemplate item)
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

    public void RequireValid(RetroPayTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayTemplate> items)
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

/// <summary>Functions of retro pay template rules.</summary>
public static class RetroPayTemplateRules
{
    public static RetroPayTemplateValidator DefaultRetroPayTemplateValidator()
    {
        return new RetroPayTemplateValidator(
            new List<IRetroPayTemplateRule>
            {
                new RetroPayTemplateNameLengthRule(1000),
                new RetroPayTemplateCodePrefixRule("ACM"),
                new RetroPayTemplateRequiredDescriptionRule("retro pay template needs an entry"),
            });
    }
}
