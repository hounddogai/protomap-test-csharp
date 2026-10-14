namespace Acme.Workforce.Orgdesign.Span.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves span policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record SpanPolicyWorkflow(ISpanPolicyStore Store, SpanPolicyValidator Validator, IClock Clock, List<string> Journal, SpanPolicyEngine Engine)
{
    public static SpanPolicyWorkflow NewSpanPolicyWorkflow(ISpanPolicyStore store, IClock clock)
    {
        return new SpanPolicyWorkflow(
            store,
            SpanPolicyRules.DefaultSpanPolicyValidator(),
            clock,
            new List<string>(),
            SpanPolicyEngine.DefaultSpanPolicyEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SpanPolicyStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SpanPolicyStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span policy {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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
