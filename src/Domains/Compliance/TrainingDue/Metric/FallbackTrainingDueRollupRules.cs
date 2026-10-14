namespace Acme.Workforce.Compliance.TrainingDue.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback training due rollup must pass. Check returns its problems, or none.</summary>
public interface IFallbackTrainingDueRollupRule
{
    List<string> Check(FallbackTrainingDueRollup item);
}

public sealed record FallbackTrainingDueRollupRequiredTitleRule(string Message) : IFallbackTrainingDueRollupRule
{
    public List<string> Check(FallbackTrainingDueRollup item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackTrainingDueRollupAllowedCodeRule(List<string> Allowed) : IFallbackTrainingDueRollupRule
{
    public List<string> Check(FallbackTrainingDueRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record FallbackTrainingDueRollupRegionLengthRule(int MaxLength) : IFallbackTrainingDueRollupRule
{
    public List<string> Check(FallbackTrainingDueRollup item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackTrainingDueRollupGraceDaysRangeRule(int Min, int Max) : IFallbackTrainingDueRollupRule
{
    public List<string> Check(FallbackTrainingDueRollup item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback training due rollup.</summary>
public sealed record FallbackTrainingDueRollupValidator(List<IFallbackTrainingDueRollupRule> Rules)
{
    public List<string> Validate(FallbackTrainingDueRollup item)
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

    public void RequireValid(FallbackTrainingDueRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackTrainingDueRollup> items)
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

/// <summary>Functions of fallback training due rollup rules.</summary>
public static class FallbackTrainingDueRollupRules
{
    public static FallbackTrainingDueRollupValidator DefaultFallbackTrainingDueRollupValidator()
    {
        return new FallbackTrainingDueRollupValidator(
            new List<IFallbackTrainingDueRollupRule>
            {
                new FallbackTrainingDueRollupRequiredTitleRule("fallback training due rollup needs a value"),
                new FallbackTrainingDueRollupAllowedCodeRule(new List<string> { "weighted", "adaptive", "rolling" }),
                new FallbackTrainingDueRollupRegionLengthRule(3600),
                new FallbackTrainingDueRollupGraceDaysRangeRule(32, 255),
            });
    }
}
