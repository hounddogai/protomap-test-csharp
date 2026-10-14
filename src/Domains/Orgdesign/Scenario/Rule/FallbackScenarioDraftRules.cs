namespace Acme.Workforce.Orgdesign.Scenario.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback scenario draft must pass. Check returns its problems, or none.</summary>
public interface IFallbackScenarioDraftRule
{
    List<string> Check(FallbackScenarioDraft item);
}

public sealed record FallbackScenarioDraftLabelLengthRule(int MaxLength) : IFallbackScenarioDraftRule
{
    public List<string> Check(FallbackScenarioDraft item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackScenarioDraftCodeLengthRule(int MaxLength) : IFallbackScenarioDraftRule
{
    public List<string> Check(FallbackScenarioDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackScenarioDraftRatingRangeRule(double Min, double Max) : IFallbackScenarioDraftRule
{
    public List<string> Check(FallbackScenarioDraft item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackScenarioDraftAllowedSalaryBandRule(List<string> Allowed) : IFallbackScenarioDraftRule
{
    public List<string> Check(FallbackScenarioDraft item)
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

public sealed record FallbackScenarioDraftRequiredLabelRule(string Message) : IFallbackScenarioDraftRule
{
    public List<string> Check(FallbackScenarioDraft item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback scenario draft.</summary>
public sealed record FallbackScenarioDraftValidator(List<IFallbackScenarioDraftRule> Rules)
{
    public List<string> Validate(FallbackScenarioDraft item)
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

    public void RequireValid(FallbackScenarioDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackScenarioDraft> items)
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

/// <summary>Functions of fallback scenario draft rules.</summary>
public static class FallbackScenarioDraftRules
{
    public static FallbackScenarioDraftValidator DefaultFallbackScenarioDraftValidator()
    {
        return new FallbackScenarioDraftValidator(
            new List<IFallbackScenarioDraftRule>
            {
                new FallbackScenarioDraftLabelLengthRule(32),
                new FallbackScenarioDraftCodeLengthRule(120),
                new FallbackScenarioDraftRatingRangeRule(6.8, 1922.7),
                new FallbackScenarioDraftAllowedSalaryBandRule(new List<string> { "pending", "seasonal" }),
                new FallbackScenarioDraftRequiredLabelRule("fallback scenario draft needs a value"),
            });
    }
}
