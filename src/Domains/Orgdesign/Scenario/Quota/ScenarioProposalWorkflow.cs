namespace Acme.Workforce.Orgdesign.Scenario.Quota;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scenario proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScenarioProposalDesk(IScenarioProposalStore Store, ScenarioProposalValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScenarioProposalStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScenarioProposalStatus.Stopped);
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

/// <summary>Functions of scenario proposal workflow.</summary>
public static class ScenarioProposalWorkflow
{
    public static ScenarioProposalDesk NewScenarioProposalDesk(IScenarioProposalStore store, IClock clock)
    {
        return new ScenarioProposalDesk(
            store,
            ScenarioProposalRules.DefaultScenarioProposalValidator(),
            clock,
            new List<string>());
    }
}
