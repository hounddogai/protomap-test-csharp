namespace Acme.Workforce.Engagement.Recognition.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default recognition policies through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultRecognitionPolicyDesk(IDefaultRecognitionPolicyStore Store, DefaultRecognitionPolicyValidator Validator, IClock Clock, List<string> Journal, DefaultRecognitionPolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default recognition policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultRecognitionPolicyStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default recognition policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultRecognitionPolicyStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default recognition policy {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of default recognition policy workflow.</summary>
public static class DefaultRecognitionPolicyWorkflow
{
    public static DefaultRecognitionPolicyDesk NewDefaultRecognitionPolicyDesk(IDefaultRecognitionPolicyStore store, IClock clock, Registry flags)
    {
        return new DefaultRecognitionPolicyDesk(
            store,
            DefaultRecognitionPolicyRules.DefaultDefaultRecognitionPolicyValidator(),
            clock,
            new List<string>(),
            DefaultRecognitionPolicyEngine.DefaultDefaultRecognitionPolicyEngine(flags));
    }
}
