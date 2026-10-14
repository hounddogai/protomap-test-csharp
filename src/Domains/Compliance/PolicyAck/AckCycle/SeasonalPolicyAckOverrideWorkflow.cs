namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal policy ack overrides through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalPolicyAckOverrideWorkflow(ISeasonalPolicyAckOverrideStore Store, SeasonalPolicyAckOverrideValidator Validator, IClock Clock, List<string> Journal, SeasonalPolicyAckOverrideEngine Engine)
{
    public static SeasonalPolicyAckOverrideWorkflow NewSeasonalPolicyAckOverrideWorkflow(ISeasonalPolicyAckOverrideStore store, IClock clock)
    {
        return new SeasonalPolicyAckOverrideWorkflow(
            store,
            SeasonalPolicyAckOverrideRules.DefaultSeasonalPolicyAckOverrideValidator(),
            clock,
            new List<string>(),
            SeasonalPolicyAckOverrideEngine.DefaultSeasonalPolicyAckOverrideEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal policy ack override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalPolicyAckOverrideStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal policy ack override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalPolicyAckOverrideStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal policy ack override {code} not found");
        }
        var result = Engine.Compute(item);
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
