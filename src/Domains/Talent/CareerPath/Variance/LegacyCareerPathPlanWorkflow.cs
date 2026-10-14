namespace Acme.Workforce.Talent.CareerPath.Variance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy career path plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyCareerPathPlanWorkflow(ILegacyCareerPathPlanStore Store, LegacyCareerPathPlanValidator Validator, IClock Clock, List<string> Journal)
{
    public static LegacyCareerPathPlanWorkflow NewLegacyCareerPathPlanWorkflow(ILegacyCareerPathPlanStore store, IClock clock)
    {
        return new LegacyCareerPathPlanWorkflow(
            store,
            LegacyCareerPathPlanRules.DefaultLegacyCareerPathPlanValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy career path plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyCareerPathPlanStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy career path plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyCareerPathPlanStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public int Scan(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            var problems = Validator.Validate(item);
            if (problems.Count == 0)
            {
                continue;
            }
            Note($"{item.Code} needs attention: {string.Join(", ", problems)}");
            changed += 1;
        }
        return changed;
    }
}
