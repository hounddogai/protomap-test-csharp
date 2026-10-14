namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary scenario segments through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryScenarioSegmentWorkflow(IPrimaryScenarioSegmentStore Store, PrimaryScenarioSegmentValidator Validator, IClock Clock, List<string> Journal)
{
    public static PrimaryScenarioSegmentWorkflow NewPrimaryScenarioSegmentWorkflow(IPrimaryScenarioSegmentStore store, IClock clock)
    {
        return new PrimaryScenarioSegmentWorkflow(
            store,
            PrimaryScenarioSegmentRules.DefaultPrimaryScenarioSegmentValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary scenario segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryScenarioSegmentStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary scenario segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryScenarioSegmentStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
