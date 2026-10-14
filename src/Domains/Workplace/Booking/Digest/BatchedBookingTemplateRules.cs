namespace Acme.Workforce.Workplace.Booking.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched booking template must pass. Check returns its problems, or none.</summary>
public interface IBatchedBookingTemplateRule
{
    List<string> Check(BatchedBookingTemplate item);
}

public sealed record BatchedBookingTemplateUpdatedAtRangeRule(int Min, int Max) : IBatchedBookingTemplateRule
{
    public List<string> Check(BatchedBookingTemplate item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedBookingTemplateAllowedSalaryBandRule(List<string> Allowed) : IBatchedBookingTemplateRule
{
    public List<string> Check(BatchedBookingTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.SalaryBand)))
        {
            return new List<string> { $"salary band {item.SalaryBand} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record BatchedBookingTemplateRateRangeRule(double Min, double Max) : IBatchedBookingTemplateRule
{
    public List<string> Check(BatchedBookingTemplate item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedBookingTemplateJobCodeLengthRule(int MaxLength) : IBatchedBookingTemplateRule
{
    public List<string> Check(BatchedBookingTemplate item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched booking template.</summary>
public sealed record BatchedBookingTemplateValidator(List<IBatchedBookingTemplateRule> Rules)
{
    public List<string> Validate(BatchedBookingTemplate item)
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

    public void RequireValid(BatchedBookingTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedBookingTemplate> items)
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

/// <summary>Functions of batched booking template rules.</summary>
public static class BatchedBookingTemplateRules
{
    public static BatchedBookingTemplateValidator DefaultBatchedBookingTemplateValidator()
    {
        return new BatchedBookingTemplateValidator(
            new List<IBatchedBookingTemplateRule>
            {
                new BatchedBookingTemplateUpdatedAtRangeRule(64, 80),
                new BatchedBookingTemplateAllowedSalaryBandRule(new List<string> { "adaptive", "regional" }),
                new BatchedBookingTemplateRateRangeRule(1.8, 2305.8),
                new BatchedBookingTemplateJobCodeLengthRule(86400),
            });
    }
}
