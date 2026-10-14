namespace Acme.Workforce.Compensation.MeritCycle.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle group is in its life cycle.</summary>
public enum MeritCycleGroupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a merit cycle group and its current status.</summary>
public sealed class MeritCycleGroup(string code, string name, Dictionary<string, string> attributes, string category, string badge, string unit, double score, MeritCycleGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Category { get; set; } = category;
    public string Badge { get; set; } = badge;
    public string Unit { get; set; } = unit;
    public double Score { get; set; } = score;
    public MeritCycleGroupStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle group in status open.</summary>
    public static MeritCycleGroup NewMeritCycleGroup(string code, string name)
    {
        return new MeritCycleGroup(
            code,
            name,
            new Dictionary<string, string>(),
            "",
            "",
            "",
            0.0,
            MeritCycleGroupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleGroupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleGroupStatus next)
    {
        switch (Status)
        {
            case MeritCycleGroupStatus.Open:
                switch (next)
                {
                    case MeritCycleGroupStatus.InReview:
                        return true;
                }
                return false;
            case MeritCycleGroupStatus.InReview:
                return next == MeritCycleGroupStatus.Resolved || next == MeritCycleGroupStatus.Archived;
            case MeritCycleGroupStatus.Resolved:
                return next == MeritCycleGroupStatus.Reopened || next == MeritCycleGroupStatus.Archived;
            case MeritCycleGroupStatus.Reopened:
                return next == MeritCycleGroupStatus.Archived;
        }
        return false;
    }

    public void MoveTo(MeritCycleGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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

    public double MeasuredValue()
    {
        return Score;
    }

    public MeritCycleGroup Copy()
    {
        return new MeritCycleGroup(Code, Name, Attributes, Category, Badge, Unit, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
