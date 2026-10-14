namespace Acme.Workforce.Orgdesign.Headcount.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves headcount overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountOverrideCoordinator(IHeadcountOverrideStore Store, HeadcountOverrideValidator Validator, IClock Clock, List<string> Journal, HeadcountOverrideEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountOverrideStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountOverrideStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount override {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of headcount override workflow.</summary>
public static class HeadcountOverrideWorkflow
{
    public static HeadcountOverrideCoordinator NewHeadcountOverrideCoordinator(IHeadcountOverrideStore store, IClock clock, Registry flags)
    {
        return new HeadcountOverrideCoordinator(
            store,
            HeadcountOverrideRules.DefaultHeadcountOverrideValidator(),
            clock,
            new List<string>(),
            HeadcountOverrideEngine.DefaultHeadcountOverrideEngine(flags));
    }
}
