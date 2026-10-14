namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default vendor scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultVendorScoreWorkflow(IDefaultVendorScoreStore Store, DefaultVendorScoreValidator Validator, IClock Clock, List<string> Journal)
{
    public static DefaultVendorScoreWorkflow NewDefaultVendorScoreWorkflow(IDefaultVendorScoreStore store, IClock clock)
    {
        return new DefaultVendorScoreWorkflow(
            store,
            DefaultVendorScoreRules.DefaultDefaultVendorScoreValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default vendor score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultVendorScoreStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default vendor score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultVendorScoreStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public int Audit(int cutoff)
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
