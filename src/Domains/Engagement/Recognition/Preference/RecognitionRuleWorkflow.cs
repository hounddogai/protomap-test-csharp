namespace Acme.Workforce.Engagement.Recognition.Preference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves recognition rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record RecognitionRuleDesk(IRecognitionRuleStore Store, RecognitionRuleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RecognitionRuleStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RecognitionRuleStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of recognition rule workflow.</summary>
public static class RecognitionRuleWorkflow
{
    public static RecognitionRuleDesk NewRecognitionRuleDesk(IRecognitionRuleStore store, IClock clock)
    {
        return new RecognitionRuleDesk(
            store,
            RecognitionRuleRules.DefaultRecognitionRuleValidator(),
            clock,
            new List<string>());
    }
}
