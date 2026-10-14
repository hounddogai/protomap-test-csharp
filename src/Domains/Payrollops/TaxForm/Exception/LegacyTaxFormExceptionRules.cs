namespace Acme.Workforce.Payrollops.TaxForm.Exception;

/// <summary>A rule a legacy tax form exception must pass. Check returns its problems, or none.</summary>
public interface ILegacyTaxFormExceptionRule
{
    List<string> Check(LegacyTaxFormException item);
}

public sealed record LegacyTaxFormExceptionTitleLengthRule(int MaxLength) : ILegacyTaxFormExceptionRule
{
    public List<string> Check(LegacyTaxFormException item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTaxFormExceptionAttemptsRangeRule(int Min, int Max) : ILegacyTaxFormExceptionRule
{
    public List<string> Check(LegacyTaxFormException item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTaxFormExceptionDurationMinutesRangeRule(int Min, int Max) : ILegacyTaxFormExceptionRule
{
    public List<string> Check(LegacyTaxFormException item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyTaxFormExceptionEndsAtRangeRule(int Min, int Max) : ILegacyTaxFormExceptionRule
{
    public List<string> Check(LegacyTaxFormException item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a legacy tax form exception.</summary>
public sealed record LegacyTaxFormExceptionValidator(List<ILegacyTaxFormExceptionRule> Rules)
{
    public List<string> Validate(LegacyTaxFormException item)
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

    public void RequireValid(LegacyTaxFormException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyTaxFormException> items)
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

/// <summary>Functions of legacy tax form exception rules.</summary>
public static class LegacyTaxFormExceptionRules
{
    public static LegacyTaxFormExceptionValidator DefaultLegacyTaxFormExceptionValidator()
    {
        return new LegacyTaxFormExceptionValidator(
            new List<ILegacyTaxFormExceptionRule>
            {
                new LegacyTaxFormExceptionTitleLengthRule(120),
                new LegacyTaxFormExceptionAttemptsRangeRule(120, 255),
                new LegacyTaxFormExceptionDurationMinutesRangeRule(255, 3600),
                new LegacyTaxFormExceptionEndsAtRangeRule(16, 16),
            });
    }
}
