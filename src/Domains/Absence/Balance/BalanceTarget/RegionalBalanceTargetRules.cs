namespace Acme.Workforce.Absence.Balance.BalanceTarget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional balance target must pass. Check returns its problems, or none.</summary>
public interface IRegionalBalanceTargetRule
{
    List<string> Check(RegionalBalanceTarget item);
}

public sealed record RegionalBalanceTargetRequiredNameRule(string Message) : IRegionalBalanceTargetRule
{
    public List<string> Check(RegionalBalanceTarget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalBalanceTargetAllowedCodeRule(List<string> Allowed) : IRegionalBalanceTargetRule
{
    public List<string> Check(RegionalBalanceTarget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBalanceTargetDurationMinutesRangeRule(int Min, int Max) : IRegionalBalanceTargetRule
{
    public List<string> Check(RegionalBalanceTarget item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBalanceTargetCodeLengthRule(int MaxLength) : IRegionalBalanceTargetRule
{
    public List<string> Check(RegionalBalanceTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional balance target.</summary>
public sealed record RegionalBalanceTargetValidator(List<IRegionalBalanceTargetRule> Rules)
{
    public List<string> Validate(RegionalBalanceTarget item)
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

    public void RequireValid(RegionalBalanceTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalBalanceTarget> items)
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

/// <summary>Functions of regional balance target rules.</summary>
public static class RegionalBalanceTargetRules
{
    public static RegionalBalanceTargetValidator DefaultRegionalBalanceTargetValidator()
    {
        return new RegionalBalanceTargetValidator(
            new List<IRegionalBalanceTargetRule>
            {
                new RegionalBalanceTargetRequiredNameRule("regional balance target needs this field"),
                new RegionalBalanceTargetAllowedCodeRule(new List<string>()),
                new RegionalBalanceTargetDurationMinutesRangeRule(80, 120),
                new RegionalBalanceTargetCodeLengthRule(16),
            });
    }
}
