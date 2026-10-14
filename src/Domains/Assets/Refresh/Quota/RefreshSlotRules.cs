namespace Acme.Workforce.Assets.Refresh.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a refresh slot must pass. Check returns its problems, or none.</summary>
public interface IRefreshSlotRule
{
    List<string> Check(RefreshSlot item);
}

public sealed record RefreshSlotActiveGuardRule(double Limit) : IRefreshSlotRule
{
    public List<string> Check(RefreshSlot item)
    {
        if (item.Active && item.Fte > Limit)
        {
            return new List<string> { $"fte {item.Fte} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record RefreshSlotFteRangeRule(double Min, double Max) : IRefreshSlotRule
{
    public List<string> Check(RefreshSlot item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshSlotAllowedCodeRule(List<string> Allowed) : IRefreshSlotRule
{
    public List<string> Check(RefreshSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record RefreshSlotRequiredNameRule(string Message) : IRefreshSlotRule
{
    public List<string> Check(RefreshSlot item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh slot.</summary>
public sealed record RefreshSlotValidator(List<IRefreshSlotRule> Rules)
{
    public List<string> Validate(RefreshSlot item)
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

    public void RequireValid(RefreshSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshSlot> items)
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

/// <summary>Functions of refresh slot rules.</summary>
public static class RefreshSlotRules
{
    public static RefreshSlotValidator DefaultRefreshSlotValidator()
    {
        return new RefreshSlotValidator(
            new List<IRefreshSlotRule>
            {
                new RefreshSlotActiveGuardRule(9.3),
                new RefreshSlotFteRangeRule(4.1, 1455.8),
                new RefreshSlotAllowedCodeRule(new List<string>()),
                new RefreshSlotRequiredNameRule("refresh slot needs this field"),
            });
    }
}
