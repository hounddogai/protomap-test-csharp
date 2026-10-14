namespace Acme.Workforce.Assets.LicenseSeat.Template;

/// <summary>A rule a license seat template must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatTemplateRule
{
    List<string> Check(LicenseSeatTemplate item);
}

public sealed record LicenseSeatTemplateUpdatedAtRangeRule(int Min, int Max) : ILicenseSeatTemplateRule
{
    public List<string> Check(LicenseSeatTemplate item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatTemplateRequiredNameRule(string Message) : ILicenseSeatTemplateRule
{
    public List<string> Check(LicenseSeatTemplate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatTemplateNameLengthRule(int MaxLength) : ILicenseSeatTemplateRule
{
    public List<string> Check(LicenseSeatTemplate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatTemplateCodePrefixRule(string Prefix) : ILicenseSeatTemplateRule
{
    public List<string> Check(LicenseSeatTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatTemplateLeaveDaysRangeRule(int Min, int Max) : ILicenseSeatTemplateRule
{
    public List<string> Check(LicenseSeatTemplate item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LicenseSeatTemplateRequiredGradeRule(string Message) : ILicenseSeatTemplateRule
{
    public List<string> Check(LicenseSeatTemplate item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat template.</summary>
public sealed record LicenseSeatTemplateValidator(List<ILicenseSeatTemplateRule> Rules)
{
    public List<string> Validate(LicenseSeatTemplate item)
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

    public void RequireValid(LicenseSeatTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatTemplate> items)
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

/// <summary>Functions of license seat template rules.</summary>
public static class LicenseSeatTemplateRules
{
    public static LicenseSeatTemplateValidator DefaultLicenseSeatTemplateValidator()
    {
        return new LicenseSeatTemplateValidator(
            new List<ILicenseSeatTemplateRule>
            {
                new LicenseSeatTemplateUpdatedAtRangeRule(16, 64),
                new LicenseSeatTemplateRequiredNameRule("license seat template needs an entry"),
                new LicenseSeatTemplateNameLengthRule(120),
                new LicenseSeatTemplateCodePrefixRule("REF"),
                new LicenseSeatTemplateLeaveDaysRangeRule(120, 3600),
                new LicenseSeatTemplateRequiredGradeRule("license seat template needs an entry"),
            });
    }
}
