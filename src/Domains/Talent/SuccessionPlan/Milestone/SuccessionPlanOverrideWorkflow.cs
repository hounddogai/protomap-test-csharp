namespace Acme.Workforce.Talent.SuccessionPlan.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves succession plan overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record SuccessionPlanOverrideWorkflow(ISuccessionPlanOverrideStore Store, SuccessionPlanOverrideValidator Validator, IClock Clock, List<string> Journal, SuccessionPlanOverrideEngine Engine)
{
    public static SuccessionPlanOverrideWorkflow NewSuccessionPlanOverrideWorkflow(ISuccessionPlanOverrideStore store, IClock clock, Registry flags)
    {
        return new SuccessionPlanOverrideWorkflow(
            store,
            SuccessionPlanOverrideRules.DefaultSuccessionPlanOverrideValidator(),
            clock,
            new List<string>(),
            SuccessionPlanOverrideEngine.DefaultSuccessionPlanOverrideEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SuccessionPlanOverrideStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SuccessionPlanOverrideStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"succession plan override {code} not found");
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
