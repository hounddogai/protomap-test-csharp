namespace Acme.Workforce.Workplace.Desk.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves desk preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeskPreferenceWorkflow(IDeskPreferenceStore Store, DeskPreferenceValidator Validator, IClock Clock, List<string> Journal, DeskPreferenceEngine Engine)
{
    public static DeskPreferenceWorkflow NewDeskPreferenceWorkflow(IDeskPreferenceStore store, IClock clock, Registry flags)
    {
        return new DeskPreferenceWorkflow(
            store,
            DeskPreferenceRules.DefaultDeskPreferenceValidator(),
            clock,
            new List<string>(),
            DeskPreferenceEngine.DefaultDeskPreferenceEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeskPreferenceStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeskPreferenceStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk preference {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.CreatedAt > cutoff)
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
