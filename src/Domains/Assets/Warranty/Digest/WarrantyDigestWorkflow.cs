namespace Acme.Workforce.Assets.Warranty.Digest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves warranty digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyDigestWorkflow(IWarrantyDigestStore Store, WarrantyDigestValidator Validator, IClock Clock, List<string> Journal, WarrantyDigestEngine Engine)
{
    public static WarrantyDigestWorkflow NewWarrantyDigestWorkflow(IWarrantyDigestStore store, IClock clock)
    {
        return new WarrantyDigestWorkflow(
            store,
            WarrantyDigestRules.DefaultWarrantyDigestValidator(),
            clock,
            new List<string>(),
            WarrantyDigestEngine.DefaultWarrantyDigestEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyDigestStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyDigestStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty digest {code} not found");
        }
        var result = Engine.Evaluate(item);
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
