namespace Acme.Workforce.Payrollops.RetroPay.Review;

/// <summary>A rule a pending retro pay rule must pass. Check returns its problems, or none.</summary>
public interface IPendingRetroPayRuleRule
{
    List<string> Check(PendingRetroPayRule item);
}

public sealed record PendingRetroPayRuleNotesLengthRule(int MaxLength) : IPendingRetroPayRuleRule
{
    public List<string> Check(PendingRetroPayRule item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingRetroPayRuleCodeLengthRule(int MaxLength) : IPendingRetroPayRuleRule
{
    public List<string> Check(PendingRetroPayRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingRetroPayRuleAttemptsRangeRule(int Min, int Max) : IPendingRetroPayRuleRule
{
    public List<string> Check(PendingRetroPayRule item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingRetroPayRuleCodePrefixRule(string Prefix) : IPendingRetroPayRuleRule
{
    public List<string> Check(PendingRetroPayRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending retro pay rule.</summary>
public sealed record PendingRetroPayRuleValidator(List<IPendingRetroPayRuleRule> Rules)
{
    public List<string> Validate(PendingRetroPayRule item)
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

    public void RequireValid(PendingRetroPayRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingRetroPayRule> items)
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

/// <summary>Functions of pending retro pay rule rules.</summary>
public static class PendingRetroPayRuleRules
{
    public static PendingRetroPayRuleValidator DefaultPendingRetroPayRuleValidator()
    {
        return new PendingRetroPayRuleValidator(
            new List<IPendingRetroPayRuleRule>
            {
                new PendingRetroPayRuleNotesLengthRule(64),
                new PendingRetroPayRuleCodeLengthRule(16),
                new PendingRetroPayRuleAttemptsRangeRule(32, 80),
                new PendingRetroPayRuleCodePrefixRule("X"),
            });
    }
}
