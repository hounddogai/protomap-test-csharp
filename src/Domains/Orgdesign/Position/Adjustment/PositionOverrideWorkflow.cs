namespace Acme.Workforce.Orgdesign.Position.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves position overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionOverrideWorkflow(IPositionOverrideStore Store, PositionOverrideValidator Validator, IClock Clock, List<string> Journal, PositionOverrideEngine Engine)
{
    public static PositionOverrideWorkflow NewPositionOverrideWorkflow(IPositionOverrideStore store, IClock clock, Registry flags)
    {
        return new PositionOverrideWorkflow(
            store,
            PositionOverrideRules.DefaultPositionOverrideValidator(),
            clock,
            new List<string>(),
            PositionOverrideEngine.DefaultPositionOverrideEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionOverrideStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionOverrideStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position override {code} not found");
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
