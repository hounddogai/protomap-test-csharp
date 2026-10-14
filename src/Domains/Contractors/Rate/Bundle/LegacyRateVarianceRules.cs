namespace Acme.Workforce.Contractors.Rate.Bundle;

/// <summary>A rule a legacy rate variance must pass. Check returns its problems, or none.</summary>
public interface ILegacyRateVarianceRule
{
    List<string> Check(LegacyRateVariance item);
}

public sealed record LegacyRateVarianceMaximumRangeRule(double Min, double Max) : ILegacyRateVarianceRule
{
    public List<string> Check(LegacyRateVariance item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRateVarianceTimezoneLengthRule(int MaxLength) : ILegacyRateVarianceRule
{
    public List<string> Check(LegacyRateVariance item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRateVariancePriorityRangeRule(int Min, int Max) : ILegacyRateVarianceRule
{
    public List<string> Check(LegacyRateVariance item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRateVarianceCodeLengthRule(int MaxLength) : ILegacyRateVarianceRule
{
    public List<string> Check(LegacyRateVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRateVarianceContactEmailLengthRule(int MaxLength) : ILegacyRateVarianceRule
{
    public List<string> Check(LegacyRateVariance item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy rate variance.</summary>
public sealed record LegacyRateVarianceValidator(List<ILegacyRateVarianceRule> Rules)
{
    public List<string> Validate(LegacyRateVariance item)
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

    public void RequireValid(LegacyRateVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyRateVariance> items)
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

/// <summary>Functions of legacy rate variance rules.</summary>
public static class LegacyRateVarianceRules
{
    public static LegacyRateVarianceValidator DefaultLegacyRateVarianceValidator()
    {
        return new LegacyRateVarianceValidator(
            new List<ILegacyRateVarianceRule>
            {
                new LegacyRateVarianceMaximumRangeRule(8.4, 1335.7),
                new LegacyRateVarianceTimezoneLengthRule(120),
                new LegacyRateVariancePriorityRangeRule(16, 32),
                new LegacyRateVarianceCodeLengthRule(3600),
                new LegacyRateVarianceContactEmailLengthRule(255),
            });
    }
}
