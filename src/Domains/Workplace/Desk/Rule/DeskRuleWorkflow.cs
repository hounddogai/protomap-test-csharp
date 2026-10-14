namespace Acme.Workforce.Workplace.Desk.Rule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves desk rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeskRuleDesk(IDeskRuleStore Store, DeskRuleValidator Validator, IClock Clock, List<string> Journal, DeskRuleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeskRuleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeskRuleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk rule {code} not found");
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

/// <summary>Functions of desk rule workflow.</summary>
public static class DeskRuleWorkflow
{
    public static DeskRuleDesk NewDeskRuleDesk(IDeskRuleStore store, IClock clock)
    {
        return new DeskRuleDesk(
            store,
            DeskRuleRules.DefaultDeskRuleValidator(),
            clock,
            new List<string>(),
            DeskRuleEngine.DefaultDeskRuleEngine());
    }
}
