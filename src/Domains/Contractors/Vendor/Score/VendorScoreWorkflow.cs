namespace Acme.Workforce.Contractors.Vendor.Score;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves vendor scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record VendorScoreWorkflow(IVendorScoreStore Store, VendorScoreValidator Validator, IClock Clock, List<string> Journal)
{
    public static VendorScoreWorkflow NewVendorScoreWorkflow(IVendorScoreStore store, IClock clock)
    {
        return new VendorScoreWorkflow(
            store,
            VendorScoreRules.DefaultVendorScoreValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VendorScoreStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"vendor score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VendorScoreStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
            if (item.UpdatedAt > cutoff)
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
