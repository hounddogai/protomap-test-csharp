namespace Acme.Workforce.Contractors.StatementOfWork.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted statement of work bundle must pass. Check returns its problems, or none.</summary>
public interface IWeightedStatementOfWorkBundleRule
{
    List<string> Check(WeightedStatementOfWorkBundle item);
}

public sealed record WeightedStatementOfWorkBundleNameLengthRule(int MaxLength) : IWeightedStatementOfWorkBundleRule
{
    public List<string> Check(WeightedStatementOfWorkBundle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedStatementOfWorkBundleRequiredUnitRule(string Message) : IWeightedStatementOfWorkBundleRule
{
    public List<string> Check(WeightedStatementOfWorkBundle item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedStatementOfWorkBundleCodeLengthRule(int MaxLength) : IWeightedStatementOfWorkBundleRule
{
    public List<string> Check(WeightedStatementOfWorkBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedStatementOfWorkBundleDurationMinutesRangeRule(int Min, int Max) : IWeightedStatementOfWorkBundleRule
{
    public List<string> Check(WeightedStatementOfWorkBundle item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedStatementOfWorkBundleAllowedPhoneRule(List<string> Allowed) : IWeightedStatementOfWorkBundleRule
{
    public List<string> Check(WeightedStatementOfWorkBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted statement of work bundle.</summary>
public sealed record WeightedStatementOfWorkBundleValidator(List<IWeightedStatementOfWorkBundleRule> Rules)
{
    public List<string> Validate(WeightedStatementOfWorkBundle item)
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

    public void RequireValid(WeightedStatementOfWorkBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedStatementOfWorkBundle> items)
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

/// <summary>Functions of weighted statement of work bundle rules.</summary>
public static class WeightedStatementOfWorkBundleRules
{
    public static WeightedStatementOfWorkBundleValidator DefaultWeightedStatementOfWorkBundleValidator()
    {
        return new WeightedStatementOfWorkBundleValidator(
            new List<IWeightedStatementOfWorkBundleRule>
            {
                new WeightedStatementOfWorkBundleNameLengthRule(16),
                new WeightedStatementOfWorkBundleRequiredUnitRule("weighted statement of work bundle needs a value"),
                new WeightedStatementOfWorkBundleCodeLengthRule(255),
                new WeightedStatementOfWorkBundleDurationMinutesRangeRule(16, 32),
                new WeightedStatementOfWorkBundleAllowedPhoneRule(new List<string> { "fallback" }),
            });
    }
}
