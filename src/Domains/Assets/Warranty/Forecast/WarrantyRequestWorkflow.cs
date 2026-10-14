namespace Acme.Workforce.Assets.Warranty.Forecast;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves warranty requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyRequestWorkflow(IWarrantyRequestStore Store, WarrantyRequestValidator Validator, IClock Clock, List<string> Journal, WarrantyRequestEngine Engine)
{
    public static WarrantyRequestWorkflow NewWarrantyRequestWorkflow(IWarrantyRequestStore store, IClock clock)
    {
        return new WarrantyRequestWorkflow(
            store,
            WarrantyRequestRules.DefaultWarrantyRequestValidator(),
            clock,
            new List<string>(),
            WarrantyRequestEngine.DefaultWarrantyRequestEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyRequestStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyRequestStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty request {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.StartsAt > cutoff)
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
