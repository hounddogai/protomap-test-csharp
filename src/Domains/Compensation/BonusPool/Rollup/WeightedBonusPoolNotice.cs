namespace Acme.Workforce.Compensation.BonusPool.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted bonus pool notice is in its life cycle.</summary>
public enum WeightedBonusPoolNoticeStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a weighted bonus pool notice and its current status.</summary>
public sealed class WeightedBonusPoolNotice(string code, string name, int limit, double maximum, Dictionary<string, string> attributes, string employeeId, WeightedBonusPoolNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public double Maximum { get; set; } = maximum;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string EmployeeId { get; set; } = employeeId;
    public WeightedBonusPoolNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a weighted bonus pool notice in status open.</summary>
    public static WeightedBonusPoolNotice NewWeightedBonusPoolNotice(string code, string name)
    {
        return new WeightedBonusPoolNotice(
            code,
            name,
            0,
            0.0,
            new Dictionary<string, string>(),
            "",
            WeightedBonusPoolNoticeStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedBonusPoolNoticeStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedBonusPoolNoticeStatus next)
    {
        switch (Status)
        {
            case WeightedBonusPoolNoticeStatus.Open:
                return next == WeightedBonusPoolNoticeStatus.Resolved || next == WeightedBonusPoolNoticeStatus.Archived;
            case WeightedBonusPoolNoticeStatus.InReview:
                switch (next)
                {
                    case WeightedBonusPoolNoticeStatus.Archived:
                        return true;
                }
                return false;
            case WeightedBonusPoolNoticeStatus.Resolved:
                switch (next)
                {
                    case WeightedBonusPoolNoticeStatus.Archived:
                        return true;
                }
                return false;
            case WeightedBonusPoolNoticeStatus.Reopened:
                switch (next)
                {
                    case WeightedBonusPoolNoticeStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedBonusPoolNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted bonus pool notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"weighted bonus pool notice"} {Code}";
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

    public double AdjustedValue()
    {
        return Math.Round((double)Limit - Maximum, 2);
    }

    public WeightedBonusPoolNotice Copy()
    {
        return new WeightedBonusPoolNotice(Code, Name, Limit, Maximum, Attributes, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
