namespace Acme.Workforce.Talent.Mobility.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default mobility targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultMobilityTargetWorkflow(IDefaultMobilityTargetStore Store, DefaultMobilityTargetValidator Validator, IClock Clock, List<string> Journal, DefaultMobilityTargetEngine Engine)
{
    public static DefaultMobilityTargetWorkflow NewDefaultMobilityTargetWorkflow(IDefaultMobilityTargetStore store, IClock clock)
    {
        return new DefaultMobilityTargetWorkflow(
            store,
            DefaultMobilityTargetRules.DefaultDefaultMobilityTargetValidator(),
            clock,
            new List<string>(),
            DefaultMobilityTargetEngine.DefaultDefaultMobilityTargetEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default mobility target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultMobilityTargetStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default mobility target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultMobilityTargetStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default mobility target {code} not found");
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
