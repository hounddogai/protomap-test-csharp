namespace Acme.Workforce.Talent.Mobility.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a mobility template must pass. Check returns its problems, or none.</summary>
public interface IMobilityTemplateRule
{
    List<string> Check(MobilityTemplate item);
}

public sealed record MobilityTemplateAllowedCodeRule(List<string> Allowed) : IMobilityTemplateRule
{
    public List<string> Check(MobilityTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

public sealed record MobilityTemplateRequiredCurrencyRule(string Message) : IMobilityTemplateRule
{
    public List<string> Check(MobilityTemplate item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityTemplateCostRangeRule(double Min, double Max) : IMobilityTemplateRule
{
    public List<string> Check(MobilityTemplate item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityTemplateArchivedGuardRule(double Limit) : IMobilityTemplateRule
{
    public List<string> Check(MobilityTemplate item)
    {
        if (item.Archived && item.Cost > Limit)
        {
            return new List<string> { $"cost {item.Cost} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility template.</summary>
public sealed record MobilityTemplateValidator(List<IMobilityTemplateRule> Rules)
{
    public List<string> Validate(MobilityTemplate item)
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

    public void RequireValid(MobilityTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityTemplate> items)
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

/// <summary>Functions of mobility template rules.</summary>
public static class MobilityTemplateRules
{
    public static MobilityTemplateValidator DefaultMobilityTemplateValidator()
    {
        return new MobilityTemplateValidator(
            new List<IMobilityTemplateRule>
            {
                new MobilityTemplateAllowedCodeRule(new List<string> { "fallback" }),
                new MobilityTemplateRequiredCurrencyRule("mobility template needs a value"),
                new MobilityTemplateCostRangeRule(8.3, 4541.1),
                new MobilityTemplateArchivedGuardRule(5.5),
            });
    }
}
