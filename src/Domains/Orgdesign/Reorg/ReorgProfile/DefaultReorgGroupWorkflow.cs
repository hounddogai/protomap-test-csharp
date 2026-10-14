namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default reorg groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultReorgGroupCoordinator(IDefaultReorgGroupStore Store, DefaultReorgGroupValidator Validator, IClock Clock, List<string> Journal, DefaultReorgGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default reorg group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultReorgGroupStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default reorg group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultReorgGroupStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default reorg group {code} not found");
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

/// <summary>Functions of default reorg group workflow.</summary>
public static class DefaultReorgGroupWorkflow
{
    public static DefaultReorgGroupCoordinator NewDefaultReorgGroupCoordinator(IDefaultReorgGroupStore store, IClock clock, Registry flags)
    {
        return new DefaultReorgGroupCoordinator(
            store,
            DefaultReorgGroupRules.DefaultDefaultReorgGroupValidator(),
            clock,
            new List<string>(),
            DefaultReorgGroupEngine.DefaultDefaultReorgGroupEngine(flags));
    }
}
