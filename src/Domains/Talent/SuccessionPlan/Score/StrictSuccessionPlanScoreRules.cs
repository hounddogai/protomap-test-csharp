namespace Acme.Workforce.Talent.SuccessionPlan.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict succession plan score must pass. Check returns its problems, or none.</summary>
public interface IStrictSuccessionPlanScoreRule
{
    List<string> Check(StrictSuccessionPlanScore item);
}

public sealed record StrictSuccessionPlanScoreAllowedCurrencyRule(List<string> Allowed) : IStrictSuccessionPlanScoreRule
{
    public List<string> Check(StrictSuccessionPlanScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record StrictSuccessionPlanScoreRequiredContactEmailRule(string Message) : IStrictSuccessionPlanScoreRule
{
    public List<string> Check(StrictSuccessionPlanScore item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictSuccessionPlanScoreCeilingRangeRule(double Min, double Max) : IStrictSuccessionPlanScoreRule
{
    public List<string> Check(StrictSuccessionPlanScore item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictSuccessionPlanScoreContactEmailLengthRule(int MaxLength) : IStrictSuccessionPlanScoreRule
{
    public List<string> Check(StrictSuccessionPlanScore item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictSuccessionPlanScoreRequiredNameRule(string Message) : IStrictSuccessionPlanScoreRule
{
    public List<string> Check(StrictSuccessionPlanScore item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictSuccessionPlanScoreCostRangeRule(double Min, double Max) : IStrictSuccessionPlanScoreRule
{
    public List<string> Check(StrictSuccessionPlanScore item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict succession plan score.</summary>
public sealed record StrictSuccessionPlanScoreValidator(List<IStrictSuccessionPlanScoreRule> Rules)
{
    public List<string> Validate(StrictSuccessionPlanScore item)
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

    public void RequireValid(StrictSuccessionPlanScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictSuccessionPlanScore> items)
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

/// <summary>Functions of strict succession plan score rules.</summary>
public static class StrictSuccessionPlanScoreRules
{
    public static StrictSuccessionPlanScoreValidator DefaultStrictSuccessionPlanScoreValidator()
    {
        return new StrictSuccessionPlanScoreValidator(
            new List<IStrictSuccessionPlanScoreRule>
            {
                new StrictSuccessionPlanScoreAllowedCurrencyRule(new List<string> { "rolling", "capped" }),
                new StrictSuccessionPlanScoreRequiredContactEmailRule(
                    "strict succession plan score needs this field"),
                new StrictSuccessionPlanScoreCeilingRangeRule(8.8, 1077.6),
                new StrictSuccessionPlanScoreContactEmailLengthRule(1000),
                new StrictSuccessionPlanScoreRequiredNameRule("strict succession plan score needs a value"),
                new StrictSuccessionPlanScoreCostRangeRule(8.0, 4029.1),
            });
    }
}
