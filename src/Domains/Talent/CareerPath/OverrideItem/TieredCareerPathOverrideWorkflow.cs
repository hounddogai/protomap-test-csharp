namespace Acme.Workforce.Talent.CareerPath.OverrideItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered career path overrides through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record TieredCareerPathOverrideService(ITieredCareerPathOverrideStore Store, TieredCareerPathOverrideValidator Validator, IClock Clock, List<string> Journal, TieredCareerPathOverrideEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered career path override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredCareerPathOverrideStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered career path override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredCareerPathOverrideStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered career path override {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of tiered career path override workflow.</summary>
public static class TieredCareerPathOverrideWorkflow
{
    public static TieredCareerPathOverrideService NewTieredCareerPathOverrideService(ITieredCareerPathOverrideStore store, IClock clock)
    {
        return new TieredCareerPathOverrideService(
            store,
            TieredCareerPathOverrideRules.DefaultTieredCareerPathOverrideValidator(),
            clock,
            new List<string>(),
            TieredCareerPathOverrideEngine.DefaultTieredCareerPathOverrideEngine());
    }
}
