namespace Acme.Workforce.Assets.Warranty.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves warranty exceptions through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyExceptionCoordinator(IWarrantyExceptionStore Store, WarrantyExceptionValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyExceptionStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyExceptionStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of warranty exception workflow.</summary>
public static class WarrantyExceptionWorkflow
{
    public static WarrantyExceptionCoordinator NewWarrantyExceptionCoordinator(IWarrantyExceptionStore store, IClock clock)
    {
        return new WarrantyExceptionCoordinator(
            store,
            WarrantyExceptionRules.DefaultWarrantyExceptionValidator(),
            clock,
            new List<string>());
    }
}
