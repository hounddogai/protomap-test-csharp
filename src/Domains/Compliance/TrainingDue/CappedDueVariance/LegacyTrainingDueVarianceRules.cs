namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy training due variance must pass. Check returns its problems, or none.</summary>
public interface ILegacyTrainingDueVarianceRule
{
    List<string> Check(LegacyTrainingDueVariance item);
}

public sealed record LegacyTrainingDueVarianceCodeLengthRule(int MaxLength) : ILegacyTrainingDueVarianceRule
{
    public List<string> Check(LegacyTrainingDueVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTrainingDueVarianceRequiredOwnerIdRule(string Message) : ILegacyTrainingDueVarianceRule
{
    public List<string> Check(LegacyTrainingDueVariance item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyTrainingDueVarianceRequiredCurrencyRule(string Message) : ILegacyTrainingDueVarianceRule
{
    public List<string> Check(LegacyTrainingDueVariance item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyTrainingDueVarianceRequiredLabelRule(string Message) : ILegacyTrainingDueVarianceRule
{
    public List<string> Check(LegacyTrainingDueVariance item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyTrainingDueVarianceCodePrefixRule(string Prefix) : ILegacyTrainingDueVarianceRule
{
    public List<string> Check(LegacyTrainingDueVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTrainingDueVarianceAllowedOwnerIdRule(List<string> Allowed) : ILegacyTrainingDueVarianceRule
{
    public List<string> Check(LegacyTrainingDueVariance item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy training due variance.</summary>
public sealed record LegacyTrainingDueVarianceValidator(List<ILegacyTrainingDueVarianceRule> Rules)
{
    public List<string> Validate(LegacyTrainingDueVariance item)
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

    public void RequireValid(LegacyTrainingDueVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyTrainingDueVariance> items)
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

/// <summary>Functions of legacy training due variance rules.</summary>
public static class LegacyTrainingDueVarianceRules
{
    public static LegacyTrainingDueVarianceValidator DefaultLegacyTrainingDueVarianceValidator()
    {
        return new LegacyTrainingDueVarianceValidator(
            new List<ILegacyTrainingDueVarianceRule>
            {
                new LegacyTrainingDueVarianceCodeLengthRule(3600),
                new LegacyTrainingDueVarianceRequiredOwnerIdRule("legacy training due variance needs this field"),
                new LegacyTrainingDueVarianceRequiredCurrencyRule("legacy training due variance needs a value"),
                new LegacyTrainingDueVarianceRequiredLabelRule("legacy training due variance needs a value"),
                new LegacyTrainingDueVarianceCodePrefixRule("OPS"),
                new LegacyTrainingDueVarianceAllowedOwnerIdRule(new List<string>()),
            });
    }
}
