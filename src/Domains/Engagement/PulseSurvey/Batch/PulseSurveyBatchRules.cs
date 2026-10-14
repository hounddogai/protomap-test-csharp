namespace Acme.Workforce.Engagement.PulseSurvey.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pulse survey batch must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyBatchRule
{
    List<string> Check(PulseSurveyBatch item);
}

public sealed record PulseSurveyBatchActiveGuardRule(int Limit) : IPulseSurveyBatchRule
{
    public List<string> Check(PulseSurveyBatch item)
    {
        if (item.Active && item.CreatedAt > Limit)
        {
            return new List<string> { $"created at {item.CreatedAt} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyBatchJobCodeLengthRule(int MaxLength) : IPulseSurveyBatchRule
{
    public List<string> Check(PulseSurveyBatch item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyBatchRequiredJobCodeRule(string Message) : IPulseSurveyBatchRule
{
    public List<string> Check(PulseSurveyBatch item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyBatchRegionLengthRule(int MaxLength) : IPulseSurveyBatchRule
{
    public List<string> Check(PulseSurveyBatch item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyBatchAllowedRegionRule(List<string> Allowed) : IPulseSurveyBatchRule
{
    public List<string> Check(PulseSurveyBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is stale" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyBatchCreatedAtRangeRule(int Min, int Max) : IPulseSurveyBatchRule
{
    public List<string> Check(PulseSurveyBatch item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey batch.</summary>
public sealed record PulseSurveyBatchValidator(List<IPulseSurveyBatchRule> Rules)
{
    public List<string> Validate(PulseSurveyBatch item)
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

    public void RequireValid(PulseSurveyBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyBatch> items)
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

/// <summary>Functions of pulse survey batch rules.</summary>
public static class PulseSurveyBatchRules
{
    public static PulseSurveyBatchValidator DefaultPulseSurveyBatchValidator()
    {
        return new PulseSurveyBatchValidator(
            new List<IPulseSurveyBatchRule>
            {
                new PulseSurveyBatchActiveGuardRule(1000),
                new PulseSurveyBatchJobCodeLengthRule(86400),
                new PulseSurveyBatchRequiredJobCodeRule("pulse survey batch needs a value"),
                new PulseSurveyBatchRegionLengthRule(255),
                new PulseSurveyBatchAllowedRegionRule(new List<string> { "batched", "primary", "fallback" }),
                new PulseSurveyBatchCreatedAtRangeRule(64, 1000),
            });
    }
}
