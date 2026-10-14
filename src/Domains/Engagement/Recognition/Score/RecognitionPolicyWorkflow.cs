namespace Acme.Workforce.Engagement.Recognition.Score;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves recognition policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record RecognitionPolicyManager(IRecognitionPolicyStore Store, RecognitionPolicyValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RecognitionPolicyStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RecognitionPolicyStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of recognition policy workflow.</summary>
public static class RecognitionPolicyWorkflow
{
    public static RecognitionPolicyManager NewRecognitionPolicyManager(IRecognitionPolicyStore store, IClock clock)
    {
        return new RecognitionPolicyManager(
            store,
            RecognitionPolicyRules.DefaultRecognitionPolicyValidator(),
            clock,
            new List<string>());
    }
}
