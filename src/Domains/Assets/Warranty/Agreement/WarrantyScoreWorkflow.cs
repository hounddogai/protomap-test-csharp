namespace Acme.Workforce.Assets.Warranty.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves warranty scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyScoreService(IWarrantyScoreStore Store, WarrantyScoreValidator Validator, IClock Clock, List<string> Journal, WarrantyScoreEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyScoreStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyScoreStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty score {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of warranty score workflow.</summary>
public static class WarrantyScoreWorkflow
{
    public static WarrantyScoreService NewWarrantyScoreService(IWarrantyScoreStore store, IClock clock, Registry flags)
    {
        return new WarrantyScoreService(
            store,
            WarrantyScoreRules.DefaultWarrantyScoreValidator(),
            clock,
            new List<string>(),
            WarrantyScoreEngine.DefaultWarrantyScoreEngine(flags));
    }
}
