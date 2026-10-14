namespace Acme.Workforce.Payrollops.TaxForm.FormBundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict tax form bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictTaxFormBundleDesk(IStrictTaxFormBundleStore Store, StrictTaxFormBundleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict tax form bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictTaxFormBundleStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict tax form bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictTaxFormBundleStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of strict tax form bundle workflow.</summary>
public static class StrictTaxFormBundleWorkflow
{
    public static StrictTaxFormBundleDesk NewStrictTaxFormBundleDesk(IStrictTaxFormBundleStore store, IClock clock)
    {
        return new StrictTaxFormBundleDesk(
            store,
            StrictTaxFormBundleRules.DefaultStrictTaxFormBundleValidator(),
            clock,
            new List<string>());
    }
}
