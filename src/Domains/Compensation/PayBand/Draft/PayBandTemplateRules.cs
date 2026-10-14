namespace Acme.Workforce.Compensation.PayBand.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pay band template must pass. Check returns its problems, or none.</summary>
public interface IPayBandTemplateRule
{
    List<string> Check(PayBandTemplate item);
}

public sealed record PayBandTemplateAllowedNameRule(List<string> Allowed) : IPayBandTemplateRule
{
    public List<string> Check(PayBandTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record PayBandTemplateRequiredNameRule(string Message) : IPayBandTemplateRule
{
    public List<string> Check(PayBandTemplate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandTemplateRequiredLocationRule(string Message) : IPayBandTemplateRule
{
    public List<string> Check(PayBandTemplate item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band template.</summary>
public sealed record PayBandTemplateValidator(List<IPayBandTemplateRule> Rules)
{
    public List<string> Validate(PayBandTemplate item)
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

    public void RequireValid(PayBandTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandTemplate> items)
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

/// <summary>Functions of pay band template rules.</summary>
public static class PayBandTemplateRules
{
    public static PayBandTemplateValidator DefaultPayBandTemplateValidator()
    {
        return new PayBandTemplateValidator(
            new List<IPayBandTemplateRule>
            {
                new PayBandTemplateAllowedNameRule(new List<string> { "adaptive", "global" }),
                new PayBandTemplateRequiredNameRule("pay band template needs this field"),
                new PayBandTemplateRequiredLocationRule("pay band template needs an entry"),
            });
    }
}
