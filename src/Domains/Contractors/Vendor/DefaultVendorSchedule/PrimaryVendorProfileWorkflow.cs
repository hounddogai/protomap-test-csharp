namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary vendor profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryVendorProfileDesk(IPrimaryVendorProfileStore Store, PrimaryVendorProfileValidator Validator, IClock Clock, List<string> Journal, PrimaryVendorProfileEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary vendor profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryVendorProfileStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary vendor profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryVendorProfileStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary vendor profile {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of primary vendor profile workflow.</summary>
public static class PrimaryVendorProfileWorkflow
{
    public static PrimaryVendorProfileDesk NewPrimaryVendorProfileDesk(IPrimaryVendorProfileStore store, IClock clock)
    {
        return new PrimaryVendorProfileDesk(
            store,
            PrimaryVendorProfileRules.DefaultPrimaryVendorProfileValidator(),
            clock,
            new List<string>(),
            PrimaryVendorProfileEngine.DefaultPrimaryVendorProfileEngine());
    }
}
