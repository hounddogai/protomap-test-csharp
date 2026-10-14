namespace Acme.Workforce.Orgdesign.Span.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span plan is in its life cycle.</summary>
public enum SpanPlanStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a span plan and its current status.</summary>
public sealed class SpanPlan(string code, string label, double maximum, double hours, string category, int retries, Dictionary<string, string> attributes, string notes, SpanPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public double Hours { get; set; } = hours;
    public string Category { get; set; } = category;
    public int Retries { get; set; } = retries;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Notes { get; set; } = notes;
    public SpanPlanStatus Status { get; set; } = status;

    /// <summary>Starts a span plan in status idle.</summary>
    public static SpanPlan NewSpanPlan(string code, string label)
    {
        return new SpanPlan(code, label, 0.0, 0.0, "", 0, new Dictionary<string, string>(), "", SpanPlanStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanPlanStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanPlanStatus next)
    {
        switch (Status)
        {
            case SpanPlanStatus.Idle:
                switch (next)
                {
                    case SpanPlanStatus.Draining:
                    case SpanPlanStatus.Stopped:
                        return true;
                }
                return false;
            case SpanPlanStatus.Warming:
                return next == SpanPlanStatus.Ready || next == SpanPlanStatus.Stopped;
            case SpanPlanStatus.Ready:
                switch (next)
                {
                    case SpanPlanStatus.Draining:
                        return true;
                }
                return false;
            case SpanPlanStatus.Draining:
                return next == SpanPlanStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SpanPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedLoad()
    {
        return Math.Round(Hours + Maximum, 2);
    }

    public SpanPlan Copy()
    {
        return new SpanPlan(Code, Label, Maximum, Hours, Category, Retries, Attributes, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
