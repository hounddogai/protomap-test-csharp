namespace Acme.Workforce.Orgdesign.Scenario.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves global scenario policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalScenarioPolicyManager(IGlobalScenarioPolicyStore Store, GlobalScenarioPolicyValidator Validator, IClock Clock, List<string> Journal, GlobalScenarioPolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global scenario policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalScenarioPolicyStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global scenario policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalScenarioPolicyStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global scenario policy {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of global scenario policy workflow.</summary>
public static class GlobalScenarioPolicyWorkflow
{
    public static GlobalScenarioPolicyManager NewGlobalScenarioPolicyManager(IGlobalScenarioPolicyStore store, IClock clock, Registry flags)
    {
        return new GlobalScenarioPolicyManager(
            store,
            GlobalScenarioPolicyRules.DefaultGlobalScenarioPolicyValidator(),
            clock,
            new List<string>(),
            GlobalScenarioPolicyEngine.DefaultGlobalScenarioPolicyEngine(flags));
    }
}
