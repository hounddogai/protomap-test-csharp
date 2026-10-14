namespace Acme.Workforce.Workplace.Desk.DeskProposal;

/// <summary>A rule a weighted desk quota must pass. Check returns its problems, or none.</summary>
public interface IWeightedDeskQuotaRule
{
    List<string> Check(WeightedDeskQuota item);
}

public sealed record WeightedDeskQuotaCodeLengthRule(int MaxLength) : IWeightedDeskQuotaRule
{
    public List<string> Check(WeightedDeskQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedDeskQuotaBatchSizeRangeRule(int Min, int Max) : IWeightedDeskQuotaRule
{
    public List<string> Check(WeightedDeskQuota item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedDeskQuotaTitleLengthRule(int MaxLength) : IWeightedDeskQuotaRule
{
    public List<string> Check(WeightedDeskQuota item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedDeskQuotaRequiredLabelRule(string Message) : IWeightedDeskQuotaRule
{
    public List<string> Check(WeightedDeskQuota item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted desk quota.</summary>
public sealed record WeightedDeskQuotaValidator(List<IWeightedDeskQuotaRule> Rules)
{
    public List<string> Validate(WeightedDeskQuota item)
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

    public void RequireValid(WeightedDeskQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedDeskQuota> items)
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

/// <summary>Functions of weighted desk quota rules.</summary>
public static class WeightedDeskQuotaRules
{
    public static WeightedDeskQuotaValidator DefaultWeightedDeskQuotaValidator()
    {
        return new WeightedDeskQuotaValidator(
            new List<IWeightedDeskQuotaRule>
            {
                new WeightedDeskQuotaCodeLengthRule(3600),
                new WeightedDeskQuotaBatchSizeRangeRule(16, 120),
                new WeightedDeskQuotaTitleLengthRule(86400),
                new WeightedDeskQuotaRequiredLabelRule("weighted desk quota needs a value"),
            });
    }
}
