namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves manual visitor entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualVisitorEntryWorkflow(IManualVisitorEntryStore Store, ManualVisitorEntryValidator Validator, IClock Clock, List<string> Journal, ManualVisitorEntryEngine Engine)
{
    public static ManualVisitorEntryWorkflow NewManualVisitorEntryWorkflow(IManualVisitorEntryStore store, IClock clock, Registry flags)
    {
        return new ManualVisitorEntryWorkflow(
            store,
            ManualVisitorEntryRules.DefaultManualVisitorEntryValidator(),
            clock,
            new List<string>(),
            ManualVisitorEntryEngine.DefaultManualVisitorEntryEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual visitor entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualVisitorEntryStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual visitor entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualVisitorEntryStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual visitor entry {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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
