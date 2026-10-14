namespace Acme.Workforce.Assets.Refresh.Policy;

/// <summary>A rule a refresh policy must pass. Check returns its problems, or none.</summary>
public interface IRefreshPolicyRule
{
    List<string> Check(RefreshPolicy item);
}

public sealed record RefreshPolicyRequiredLabelRule(string Message) : IRefreshPolicyRule
{
    public List<string> Check(RefreshPolicy item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshPolicyEndsAtRangeRule(int Min, int Max) : IRefreshPolicyRule
{
    public List<string> Check(RefreshPolicy item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshPolicyUnitLengthRule(int MaxLength) : IRefreshPolicyRule
{
    public List<string> Check(RefreshPolicy item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshPolicyRequiredCodeRule(string Message) : IRefreshPolicyRule
{
    public List<string> Check(RefreshPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh policy.</summary>
public sealed record RefreshPolicyValidator(List<IRefreshPolicyRule> Rules)
{
    public List<string> Validate(RefreshPolicy item)
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

    public void RequireValid(RefreshPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshPolicy> items)
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

/// <summary>Functions of refresh policy rules.</summary>
public static class RefreshPolicyRules
{
    public static RefreshPolicyValidator DefaultRefreshPolicyValidator()
    {
        return new RefreshPolicyValidator(
            new List<IRefreshPolicyRule>
            {
                new RefreshPolicyRequiredLabelRule("refresh policy needs a value"),
                new RefreshPolicyEndsAtRangeRule(16, 120),
                new RefreshPolicyUnitLengthRule(86400),
                new RefreshPolicyRequiredCodeRule("refresh policy needs a value"),
            });
    }
}
