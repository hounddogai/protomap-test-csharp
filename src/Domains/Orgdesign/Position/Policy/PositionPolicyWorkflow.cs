namespace Acme.Workforce.Orgdesign.Position.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves position policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionPolicyManager(IPositionPolicyStore Store, PositionPolicyValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionPolicyStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionPolicyStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of position policy workflow.</summary>
public static class PositionPolicyWorkflow
{
    public static PositionPolicyManager NewPositionPolicyManager(IPositionPolicyStore store, IClock clock)
    {
        return new PositionPolicyManager(
            store,
            PositionPolicyRules.DefaultPositionPolicyValidator(),
            clock,
            new List<string>());
    }
}
