namespace Acme.Workforce.Payrollops.RetroPay.Tier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual retro pay tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualRetroPayTierService(IManualRetroPayTierStore Store, ManualRetroPayTierValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual retro pay tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualRetroPayTierStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual retro pay tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualRetroPayTierStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of manual retro pay tier workflow.</summary>
public static class ManualRetroPayTierWorkflow
{
    public static ManualRetroPayTierService NewManualRetroPayTierService(IManualRetroPayTierStore store, IClock clock)
    {
        return new ManualRetroPayTierService(
            store,
            ManualRetroPayTierRules.DefaultManualRetroPayTierValidator(),
            clock,
            new List<string>());
    }
}
