namespace Acme.Workforce.Absence.Policy.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient policy threshold must pass. Check returns its problems, or none.</summary>
public interface ILenientPolicyThresholdRule
{
    List<string> Check(LenientPolicyThreshold item);
}

public sealed record LenientPolicyThresholdAllowedCodeRule(List<string> Allowed) : ILenientPolicyThresholdRule
{
    public List<string> Check(LenientPolicyThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record LenientPolicyThresholdRequiredCodeRule(string Message) : ILenientPolicyThresholdRule
{
    public List<string> Check(LenientPolicyThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPolicyThresholdNameLengthRule(int MaxLength) : ILenientPolicyThresholdRule
{
    public List<string> Check(LenientPolicyThreshold item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPolicyThresholdGraceDaysRangeRule(int Min, int Max) : ILenientPolicyThresholdRule
{
    public List<string> Check(LenientPolicyThreshold item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient policy threshold.</summary>
public sealed record LenientPolicyThresholdValidator(List<ILenientPolicyThresholdRule> Rules)
{
    public List<string> Validate(LenientPolicyThreshold item)
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

    public void RequireValid(LenientPolicyThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientPolicyThreshold> items)
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

/// <summary>Functions of lenient policy threshold rules.</summary>
public static class LenientPolicyThresholdRules
{
    public static LenientPolicyThresholdValidator DefaultLenientPolicyThresholdValidator()
    {
        return new LenientPolicyThresholdValidator(
            new List<ILenientPolicyThresholdRule>
            {
                new LenientPolicyThresholdAllowedCodeRule(new List<string> { "legacy", "rolling", "default" }),
                new LenientPolicyThresholdRequiredCodeRule("lenient policy threshold needs an entry"),
                new LenientPolicyThresholdNameLengthRule(3600),
                new LenientPolicyThresholdGraceDaysRangeRule(80, 3600),
            });
    }
}
