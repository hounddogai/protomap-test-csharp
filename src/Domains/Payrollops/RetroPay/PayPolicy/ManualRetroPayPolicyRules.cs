namespace Acme.Workforce.Payrollops.RetroPay.PayPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual retro pay policy must pass. Check returns its problems, or none.</summary>
public interface IManualRetroPayPolicyRule
{
    List<string> Check(ManualRetroPayPolicy item);
}

public sealed record ManualRetroPayPolicyAllowedTitleRule(List<string> Allowed) : IManualRetroPayPolicyRule
{
    public List<string> Check(ManualRetroPayPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record ManualRetroPayPolicyRequiredTitleRule(string Message) : IManualRetroPayPolicyRule
{
    public List<string> Check(ManualRetroPayPolicy item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualRetroPayPolicyCodePrefixRule(string Prefix) : IManualRetroPayPolicyRule
{
    public List<string> Check(ManualRetroPayPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualRetroPayPolicyMaximumRangeRule(double Min, double Max) : IManualRetroPayPolicyRule
{
    public List<string> Check(ManualRetroPayPolicy item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a manual retro pay policy.</summary>
public sealed record ManualRetroPayPolicyValidator(List<IManualRetroPayPolicyRule> Rules)
{
    public List<string> Validate(ManualRetroPayPolicy item)
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

    public void RequireValid(ManualRetroPayPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualRetroPayPolicy> items)
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

/// <summary>Functions of manual retro pay policy rules.</summary>
public static class ManualRetroPayPolicyRules
{
    public static ManualRetroPayPolicyValidator DefaultManualRetroPayPolicyValidator()
    {
        return new ManualRetroPayPolicyValidator(
            new List<IManualRetroPayPolicyRule>
            {
                new ManualRetroPayPolicyAllowedTitleRule(new List<string> { "capped" }),
                new ManualRetroPayPolicyRequiredTitleRule("manual retro pay policy needs this field"),
                new ManualRetroPayPolicyCodePrefixRule("Q"),
                new ManualRetroPayPolicyMaximumRangeRule(6.4, 4263.0),
            });
    }
}
