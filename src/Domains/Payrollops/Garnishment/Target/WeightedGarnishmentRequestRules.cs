namespace Acme.Workforce.Payrollops.Garnishment.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted garnishment request must pass. Check returns its problems, or none.</summary>
public interface IWeightedGarnishmentRequestRule
{
    List<string> Check(WeightedGarnishmentRequest item);
}

public sealed record WeightedGarnishmentRequestDescriptionLengthRule(int MaxLength) : IWeightedGarnishmentRequestRule
{
    public List<string> Check(WeightedGarnishmentRequest item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedGarnishmentRequestCodePrefixRule(string Prefix) : IWeightedGarnishmentRequestRule
{
    public List<string> Check(WeightedGarnishmentRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedGarnishmentRequestMinimumRangeRule(double Min, double Max) : IWeightedGarnishmentRequestRule
{
    public List<string> Check(WeightedGarnishmentRequest item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedGarnishmentRequestAllowedDescriptionRule(List<string> Allowed) : IWeightedGarnishmentRequestRule
{
    public List<string> Check(WeightedGarnishmentRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is too long" };
        }
        return new List<string>();
    }
}

public sealed record WeightedGarnishmentRequestNotesLengthRule(int MaxLength) : IWeightedGarnishmentRequestRule
{
    public List<string> Check(WeightedGarnishmentRequest item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted garnishment request.</summary>
public sealed record WeightedGarnishmentRequestValidator(List<IWeightedGarnishmentRequestRule> Rules)
{
    public List<string> Validate(WeightedGarnishmentRequest item)
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

    public void RequireValid(WeightedGarnishmentRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedGarnishmentRequest> items)
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

/// <summary>Functions of weighted garnishment request rules.</summary>
public static class WeightedGarnishmentRequestRules
{
    public static WeightedGarnishmentRequestValidator DefaultWeightedGarnishmentRequestValidator()
    {
        return new WeightedGarnishmentRequestValidator(
            new List<IWeightedGarnishmentRequestRule>
            {
                new WeightedGarnishmentRequestDescriptionLengthRule(1000),
                new WeightedGarnishmentRequestCodePrefixRule("REF"),
                new WeightedGarnishmentRequestMinimumRangeRule(2.6, 3460.9),
                new WeightedGarnishmentRequestAllowedDescriptionRule(
                    new List<string> { "capped", "manual", "seasonal" }),
                new WeightedGarnishmentRequestNotesLengthRule(64),
            });
    }
}
