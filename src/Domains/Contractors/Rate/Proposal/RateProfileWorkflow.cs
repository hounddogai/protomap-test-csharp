namespace Acme.Workforce.Contractors.Rate.Proposal;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rate profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record RateProfileManager(IRateProfileStore Store, RateProfileValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rate profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RateProfileStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rate profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RateProfileStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of rate profile workflow.</summary>
public static class RateProfileWorkflow
{
    public static RateProfileManager NewRateProfileManager(IRateProfileStore store, IClock clock)
    {
        return new RateProfileManager(
            store,
            RateProfileRules.DefaultRateProfileValidator(),
            clock,
            new List<string>());
    }
}
