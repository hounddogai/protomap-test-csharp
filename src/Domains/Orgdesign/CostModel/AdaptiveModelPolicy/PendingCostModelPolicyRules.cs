namespace Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

/// <summary>A rule a pending cost model policy must pass. Check returns its problems, or none.</summary>
public interface IPendingCostModelPolicyRule
{
    List<string> Check(PendingCostModelPolicy item);
}

public sealed record PendingCostModelPolicyCodeLengthRule(int MaxLength) : IPendingCostModelPolicyRule
{
    public List<string> Check(PendingCostModelPolicy item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingCostModelPolicyLabelLengthRule(int MaxLength) : IPendingCostModelPolicyRule
{
    public List<string> Check(PendingCostModelPolicy item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingCostModelPolicyArchivedGuardRule(double Limit) : IPendingCostModelPolicyRule
{
    public List<string> Check(PendingCostModelPolicy item)
    {
        if (item.Archived && item.Minimum > Limit)
        {
            return new List<string> { $"minimum {item.Minimum} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record PendingCostModelPolicyRatingRangeRule(double Min, double Max) : IPendingCostModelPolicyRule
{
    public List<string> Check(PendingCostModelPolicy item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingCostModelPolicyRequiredCodeRule(string Message) : IPendingCostModelPolicyRule
{
    public List<string> Check(PendingCostModelPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending cost model policy.</summary>
public sealed record PendingCostModelPolicyValidator(List<IPendingCostModelPolicyRule> Rules)
{
    public List<string> Validate(PendingCostModelPolicy item)
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

    public void RequireValid(PendingCostModelPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingCostModelPolicy> items)
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

/// <summary>Functions of pending cost model policy rules.</summary>
public static class PendingCostModelPolicyRules
{
    public static PendingCostModelPolicyValidator DefaultPendingCostModelPolicyValidator()
    {
        return new PendingCostModelPolicyValidator(
            new List<IPendingCostModelPolicyRule>
            {
                new PendingCostModelPolicyCodeLengthRule(120),
                new PendingCostModelPolicyLabelLengthRule(3600),
                new PendingCostModelPolicyArchivedGuardRule(1.5),
                new PendingCostModelPolicyRatingRangeRule(1.6, 244.3),
                new PendingCostModelPolicyRequiredCodeRule("pending cost model policy needs this field"),
            });
    }
}
