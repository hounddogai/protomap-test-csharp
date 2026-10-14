namespace Acme.Workforce.Compensation.Adjustment.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment template is in its life cycle.</summary>
public enum AdjustmentTemplateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A adjustment template. Its code is unique within a tenant.</summary>
public sealed class AdjustmentTemplate(string code, string name, List<string> tags, int tenureMonths, int limit, AdjustmentTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Limit { get; set; } = limit;
    public AdjustmentTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment template in status proposed.</summary>
    public static AdjustmentTemplate NewAdjustmentTemplate(string code, string name)
    {
        return new AdjustmentTemplate(code, name, new List<string>(), 0, 0, AdjustmentTemplateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentTemplateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentTemplateStatus next)
    {
        switch (Status)
        {
            case AdjustmentTemplateStatus.Proposed:
                return next == AdjustmentTemplateStatus.Rejected || next == AdjustmentTemplateStatus.Withdrawn;
            case AdjustmentTemplateStatus.Approved:
                return next == AdjustmentTemplateStatus.Withdrawn;
            case AdjustmentTemplateStatus.Rejected:
                switch (next)
                {
                    case AdjustmentTemplateStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdjustmentTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Limit - (double)TenureMonths, 2);
    }

    public AdjustmentTemplate Copy()
    {
        return new AdjustmentTemplate(Code, Name, Tags, TenureMonths, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
