namespace Acme.Workforce.Orgdesign.Scenario.Segment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scenario policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScenarioPolicyWorkflow(IScenarioPolicyStore Store, ScenarioPolicyValidator Validator, IClock Clock, List<string> Journal)
{
    public static ScenarioPolicyWorkflow NewScenarioPolicyWorkflow(IScenarioPolicyStore store, IClock clock)
    {
        return new ScenarioPolicyWorkflow(
            store,
            ScenarioPolicyRules.DefaultScenarioPolicyValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScenarioPolicyStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScenarioPolicyStatus.Stopped);
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
