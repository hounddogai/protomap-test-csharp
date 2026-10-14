namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary nomination policy must pass. Check returns its problems, or none.</summary>
public interface IPrimaryNominationPolicyRule
{
    List<string> Check(PrimaryNominationPolicy item);
}

public sealed record PrimaryNominationPolicyLocationLengthRule(int MaxLength) : IPrimaryNominationPolicyRule
{
    public List<string> Check(PrimaryNominationPolicy item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryNominationPolicyTitleLengthRule(int MaxLength) : IPrimaryNominationPolicyRule
{
    public List<string> Check(PrimaryNominationPolicy item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryNominationPolicyRequiredLocationRule(string Message) : IPrimaryNominationPolicyRule
{
    public List<string> Check(PrimaryNominationPolicy item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryNominationPolicyBudgetRangeRule(double Min, double Max) : IPrimaryNominationPolicyRule
{
    public List<string> Check(PrimaryNominationPolicy item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryNominationPolicyAllowedTitleRule(List<string> Allowed) : IPrimaryNominationPolicyRule
{
    public List<string> Check(PrimaryNominationPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary nomination policy.</summary>
public sealed record PrimaryNominationPolicyValidator(List<IPrimaryNominationPolicyRule> Rules)
{
    public List<string> Validate(PrimaryNominationPolicy item)
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

    public void RequireValid(PrimaryNominationPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryNominationPolicy> items)
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

/// <summary>Functions of primary nomination policy rules.</summary>
public static class PrimaryNominationPolicyRules
{
    public static PrimaryNominationPolicyValidator DefaultPrimaryNominationPolicyValidator()
    {
        return new PrimaryNominationPolicyValidator(
            new List<IPrimaryNominationPolicyRule>
            {
                new PrimaryNominationPolicyLocationLengthRule(80),
                new PrimaryNominationPolicyTitleLengthRule(16),
                new PrimaryNominationPolicyRequiredLocationRule("primary nomination policy needs an entry"),
                new PrimaryNominationPolicyBudgetRangeRule(2.7, 1302.2),
                new PrimaryNominationPolicyAllowedTitleRule(new List<string> { "primary", "regional", "fallback" }),
            });
    }
}
